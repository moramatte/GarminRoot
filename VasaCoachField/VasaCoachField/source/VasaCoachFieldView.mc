import Toybox.Activity;
import Toybox.Application;
import Toybox.Communications;
import Toybox.Lang;
import Toybox.Time;
import Toybox.WatchUi;

class VasaCoachFieldView extends WatchUi.DataField {
    // Removed throttle guards
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
    
    var currentDelta as String = "";
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
        // Fetch data every 30 seconds
        if (currentTime - lastFetchTime > 30) {
            fetchDataFromServer(info);
            lastFetchTime = currentTime;
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
        // var race = Application.Properties.getValue("race");
        // var dryRun = Application.Properties.getValue("dryRun");

        var race = "vasaloppet";
        var dryRun = false;
        
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
        
        // Removed throttle guards
        Communications.makeWebRequest(url, null, options, method(:onReceive));
    }
    
    function onReceive(responseCode as Number, data as Dictionary or String or Null) as Void {
        // Removed throttle guards
        if (responseCode == 200 && data != null) {
            // Parse JSON response with "newSpeed" and "leaderDistanceKm"
            if (data instanceof Dictionary) {
                var newSpeed = data.get("newSpeed");
                if (newSpeed != null) {
                    if (newSpeed instanceof String) {
                        currentDelta = newSpeed;
                    } else {
                        // fallback for legacy numeric values
                        currentDelta = newSpeed + "";
                    }
                } else {
                    currentDelta = "ERR";
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
                currentDelta = "ERR";
                leaderDistanceKm = 0.0;
            }
        } else {
            currentDelta = "ERR";
            leaderDistanceKm = 0.0;
        }

        if (currentDelta != null && currentDelta instanceof String && currentDelta.find("ERR") == 0) {
            // Log error code
            System.println("Error fetching data: " + currentDelta);
        }
    }

    function onUpdate(dc) as Void {
        // Clear background
        dc.clear();
        // Draw new tempo (large font)
        var tempoText = "--:--";
        if (currentDelta != null && currentDelta instanceof String && currentDelta.length() > 0 && currentDelta.find(":") != -1) {
            tempoText = currentDelta;
        } else if (currentDelta != null && currentDelta instanceof String && currentDelta.find("ERR") == 0) {
            tempoText = currentDelta;
        } else {
            tempoText = "--:--";
        }
        // Adapt foreground color to background: if background is black, use white text; else use black text
        // Use DataField.getBackgroundColor() to set best foreground color
        var bgColor = getBackgroundColor();
        var fgColor = (bgColor == Graphics.COLOR_BLACK) ? Graphics.COLOR_WHITE : Graphics.COLOR_BLACK;
        var w = dc.getWidth();
        var h = dc.getHeight();

        // Draw new tempo (large font) in the upper center, with unit
        var tempoY = h/2 - 35;
        var liveText = "";
        var liveFont = Graphics.FONT_SMALL;
        var liveColor = Graphics.COLOR_DK_GRAY;
        var tempoFont = Graphics.FONT_LARGE;
        // Draw tempoText centered as before
        dc.setColor(fgColor, Graphics.COLOR_TRANSPARENT);
        dc.drawText(w/2, tempoY, tempoFont, tempoText, Graphics.TEXT_JUSTIFY_CENTER);
        // Draw LIVE indicator further left, not affecting tempo centering
        if (isLive) {
            liveText = "LIVE";
            dc.setColor(liveColor, Graphics.COLOR_TRANSPARENT);
            var liveWidth = dc.getTextWidthInPixels(liveText, liveFont);
            var liveX = w/2 - dc.getTextWidthInPixels(tempoText, tempoFont)/2 - liveWidth - 16;
            dc.drawText(liveX, tempoY, liveFont, liveText, Graphics.TEXT_JUSTIFY_LEFT);
        }
        var unitText = "min/km";
        var tempoWidth = dc.getTextWidthInPixels(tempoText, tempoFont);
        var unitX = w/2 + tempoWidth/2 + 8;
        if (!healthStatusOk) {
            dc.setColor(Graphics.COLOR_RED, Graphics.COLOR_TRANSPARENT);
        } else {
            dc.setColor(fgColor, Graphics.COLOR_TRANSPARENT);
        }
        dc.drawText(unitX, tempoY, tempoFont, unitText, Graphics.TEXT_JUSTIFY_LEFT);

        // Draw leader distance (small font) further below
        var leaderText = "Leader: -- km";
        if (leaderDistanceKm >= 0) {
            leaderText = "Leader: " + leaderDistanceKm.format("%.2f") + " km";
        } else if (leaderDistanceKm < 0) {
            leaderText = "Race Complete";
        }
        var leaderY = tempoY + 55;
        dc.setColor(fgColor, Graphics.COLOR_TRANSPARENT);
        dc.drawText(w/2, leaderY, Graphics.FONT_SMALL, leaderText, Graphics.TEXT_JUSTIFY_CENTER);
    }

}