import Toybox.Activity;
import Toybox.Application;
import Toybox.Communications;
import Toybox.Lang;
import Toybox.Time;
import Toybox.WatchUi;

class VasaCoachFieldView extends WatchUi.DataField {

    var leaderDistanceKm as Float = 0.0;

    // Set the label of the data field here.
    function initialize() {
        DataField.initialize();
    }
    
    var currentDelta as Float = 0.0;
    var lastFetchTime as Number = 0;

    // The given info object contains all the current workout
    // information. Calculate a value and return it in this method.
    // Note that compute() and onUpdate() are asynchronous, and there is no
    // guarantee that compute() will be called before onUpdate().
    function compute(info as Activity.Info) as Numeric or Duration or String or Null {
        // Fetch data every 5 seconds
        var currentTime = Time.now().value();
        if (currentTime - lastFetchTime > 5) {
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
        var race = Application.Properties.getValue("race");
        var dryRun = Application.Properties.getValue("dryRun");
        
        var url = "https://vasalivefeeder.azurewebsites.net/api/TempoDelta?race=" + race + "&km=" + distanceKm + "&speed=" + speed + "&dryRun=" + dryRun;
        
        var options = {
            :method => Communications.HTTP_REQUEST_METHOD_GET
        };
        
        Communications.makeWebRequest(url, null, options, method(:onReceive));
    }
    
    function onReceive(responseCode as Number, data as Dictionary or String or Null) as Void {
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
            } else {
                currentDelta = -7777.0;
                leaderDistanceKm = 0.0;
            }
        } else {
            currentDelta = responseCode.toFloat();
            leaderDistanceKm = 0.0;
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
            tempoText = minutes.format("%d") + ":" + seconds.format("%02d");
        }
        // Adapt foreground color to background: if background is black, use white text; else use black text
        // Use DataField.getBackgroundColor() to set best foreground color
        var bgColor = getBackgroundColor();
        var fgColor = (bgColor == Graphics.COLOR_BLACK) ? Graphics.COLOR_WHITE : Graphics.COLOR_BLACK;
        dc.setColor(fgColor, Graphics.COLOR_TRANSPARENT);
        var w = dc.getWidth();
        var h = dc.getHeight();

        // Draw new tempo (large font) in the upper center, with unit
        var tempoY = h/2 - 35;
        var tempoWithUnit = tempoText + " min/km";
        dc.drawText(w/2, tempoY, Graphics.FONT_LARGE, tempoWithUnit, Graphics.TEXT_JUSTIFY_CENTER);

        // Draw leader distance (small font) further below
        var leaderText = "Leader: -- km";
        if (leaderDistanceKm > 0) {
            leaderText = "Leader: " + leaderDistanceKm.format("%.2f") + " km";
        }
        var leaderY = tempoY + 55;
        dc.drawText(w/2, leaderY, Graphics.FONT_SMALL, leaderText, Graphics.TEXT_JUSTIFY_CENTER);
    }

}