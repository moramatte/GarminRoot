import Toybox.Activity;
import Toybox.Application;
import Toybox.Communications;
import Toybox.Lang;
import Toybox.Time;
import Toybox.WatchUi;

class VasaCoachFieldView extends WatchUi.DataField {
            var tempoRequestStartTime as Number = 0;
        var tempoRequestInProgress as Boolean = false;
    var healthStatusOk as Boolean = true;
    var lastHealthCheckTime as Number = 0;

    function performHealthCheck() as Void {
        var url = "https://vasalivefeeder.azurewebsites.net/api/health";
        var options = {
            :method => Communications.HTTP_REQUEST_METHOD_GET
        };
        Communications.makeWebRequest(url, null, options, method(:onHealthCheckResponse));
    }

    function onHealthCheckResponse(responseCode as Number, data as Dictionary or String or Null) as Void {
        if (responseCode == 200) {
            healthStatusOk = true;
        } else {
            healthStatusOk = false;
        }
    }    

    var leaderDistanceKm as Float = 0.0;
    var lastRequestUrl as String = "";

    // Set the label of the data field here.
    function initialize() {
        DataField.initialize();
    }
    
    var currentDelta as Float = 0.0;
    var lastFetchTime as Number = 0;
    var isLive as Boolean = false;

    

    // The given info object contains all the current workout
    // information. Calculate a value and return it in this method.
    // Note that compute() and onUpdate() are asynchronous, and there is no
    // guarantee that compute() will be called before onUpdate().
    function compute(info as Activity.Info) as Numeric or Duration or String or Null {
        var currentTime = Time.now().value();
        // Perform health check every 10 minutes (600 seconds)
        if (currentTime - lastHealthCheckTime > 600) {
            performHealthCheck();
            lastHealthCheckTime = currentTime;
        }
        // Fetch data every 10 seconds, only if no request is in progress
        if ((currentTime - lastFetchTime > 10) && !tempoRequestInProgress) {
            fetchDataFromServer(info);
            lastFetchTime = currentTime;
        }
        // Reset guard if no response in 3 minutes (180 seconds)
        if (tempoRequestInProgress && (currentTime - tempoRequestStartTime > 180)) {
            tempoRequestInProgress = false;
        }
        // DataField expects a value, but we want to force redraw, so return 0
        return 0;
    }
    
    function fetchDataFromServer(info as Activity.Info) as Void {
        // Get current distance in kilometers (elapsedDistance is in meters)
        var distanceKm = 0.0;
        if (info.elapsedDistance != null) {
            distanceKm = info.elapsedDistance / 1000.0;
        }
        
        // Get average speed in m/s
        var speed = 0.0;
        if (info.averageSpeed != null) {
            speed = info.averageSpeed;
        }

        // Read settings
        var race = Application.Properties.getValue("race");
        var dryRun = Application.Properties.getValue("dryRun");
        
            // Get elapsed time in minutes (milliseconds to minutes)
            var elapsedMinutes = 0.0;
            if (info.elapsedTime != null) {
                elapsedMinutes = info.elapsedTime / 60000.0;
            }

            if (elapsedMinutes == 0.000000){
                elapsedMinutes = 0.000001;
            }            

            var url = "https://vasalivefeeder.azurewebsites.net/api/TempoDelta?race=" + race + "&km=" + distanceKm + "&speed=" + speed + "&elapsed=" + elapsedMinutes + "&dryRun=" + dryRun;
            lastRequestUrl = url;
        
        var options = {
            :method => Communications.HTTP_REQUEST_METHOD_GET
        };
        
        tempoRequestInProgress = true;
        tempoRequestStartTime = Time.now().value();
        Communications.makeWebRequest(url, null, options, method(:onReceive));
    }
    
    function onReceive(responseCode as Number, data as Dictionary or String or Null) as Void {
            tempoRequestInProgress = false;
        if (responseCode == 200 && data != null) {
            // Parse JSON response with "newSpeed" and "leaderDistanceKm"
            if (data instanceof Dictionary) {
                var newSpeed = data.get("newSpeed");
                if (newSpeed != null) {
                    if (newSpeed instanceof Number) {
                        currentDelta = newSpeed.toFloat();
                    } else if (newSpeed instanceof Float) {
                        currentDelta = newSpeed;
                    } else {
                        currentDelta = -8888.0;
                    }
                } else {
                    currentDelta = -9999.0;
                }
                var leaderDist = data.get("leaderDistanceKm");
                if (leaderDist != null) {
                    if (leaderDist instanceof Number) {
                        leaderDistanceKm = leaderDist.toFloat();
                    } else if (leaderDist instanceof Float) {
                        leaderDistanceKm = leaderDist;
                    } else {
                        leaderDistanceKm = 0.0;
                    }
                } else {
                    leaderDistanceKm = 0.0;
                }
                var liveStatus = data.get("live");
                if (liveStatus != null && liveStatus instanceof Boolean) {
                    isLive = liveStatus;
                } else {
                    isLive = false;
                }
            } else {
                currentDelta = -7777.0;
                leaderDistanceKm = 0.0;
            }
        } else {
            currentDelta = responseCode.toFloat();
            leaderDistanceKm = 0.0;
        }

        if (currentDelta < 0) {
            // Log error code
            System.println("Error fetching data: " + currentDelta.format("%d"));
        }
    }

    function onUpdate(dc) as Void {
        // Clear background
        dc.clear();
        // Draw new tempo (large font)
        var tempoText = "--:--";
        if (currentDelta > 0) {
            var minutes = currentDelta.toNumber();
            var seconds = ((currentDelta - minutes) * 60).toNumber();
            // Ensure seconds is within valid range
            if (seconds < 0) {
                seconds = 0;
            } else if (seconds > 59) {
                seconds = 59;
            }
            tempoText = minutes.format("%d") + ":" + seconds.format("%02d");
        } else if (currentDelta < 0 && currentDelta > -1000) {
            // Handle special error codes
            tempoText = "ERR" + (-currentDelta).format("%d");
        }
        // Adapt foreground color to background: if background is black, use white text; else use black text
        // Use DataField.getBackgroundColor() to set best foreground color
        var bgColor = getBackgroundColor();
        var fgColor = (bgColor == Graphics.COLOR_BLACK) ? Graphics.COLOR_WHITE : Graphics.COLOR_BLACK;
        var w = dc.getWidth();
        var h = dc.getHeight();

        // Draw new tempo (large font) in the upper center, with unit
        var tempoY = h/2 - 35;
        // Draw tempoText centered, then draw 'min/km' in red if health check fails
        dc.setColor(fgColor, Graphics.COLOR_TRANSPARENT);
        dc.drawText(w/2, tempoY, Graphics.FONT_LARGE, tempoText, Graphics.TEXT_JUSTIFY_CENTER);
        var unitText = "min/km";
        // Draw unit right after tempoText, offset to the right
        var tempoWidth = dc.getTextWidthInPixels(tempoText, Graphics.FONT_LARGE);
        var unitX = w/2 + tempoWidth/2 + 8;
        if (!healthStatusOk) {
            dc.setColor(Graphics.COLOR_RED, Graphics.COLOR_TRANSPARENT);
        } else {
            dc.setColor(fgColor, Graphics.COLOR_TRANSPARENT);
        }
        dc.drawText(unitX, tempoY, Graphics.FONT_LARGE, unitText, Graphics.TEXT_JUSTIFY_LEFT);

        // Draw leader distance (small font) further below
        var leaderText = "Leader: -- km";
        if (leaderDistanceKm > 0) {
            leaderText = "Leader: " + leaderDistanceKm.format("%.2f") + " km";
        } else if (leaderDistanceKm < 0) {
            // Handle case where we're past the race distance
            leaderText = "Race Complete";
        }
        var leaderY = tempoY + 55;        
        
        // Draw live indicator if live data is available (left of Leader text)
        if (isLive) {
            dc.setColor(Graphics.COLOR_DK_GRAY, Graphics.COLOR_TRANSPARENT);
            dc.drawText(w/2 - dc.getTextWidthInPixels(leaderText, Graphics.FONT_SMALL)/2 - 15, leaderY, Graphics.FONT_XTINY, "LIVE", Graphics.TEXT_JUSTIFY_RIGHT);
        }
        
        dc.setColor(fgColor, Graphics.COLOR_TRANSPARENT);
        dc.drawText(w/2, leaderY, Graphics.FONT_SMALL, leaderText, Graphics.TEXT_JUSTIFY_CENTER);
    }

}