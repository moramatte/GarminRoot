import Toybox.Activity;
import Toybox.Application;
import Toybox.Communications;
import Toybox.Lang;
import Toybox.Time;
import Toybox.WatchUi;

class VasaCoachFieldView extends WatchUi.DataField {
    const RACE_SLUGS = [
        "bad-gastein-prologue",
        "sportgastein-criterium",
        "bad-gastein-itt-2",
        "bad-gastein-criterium",
        "engadin-la-diagonela",
        "zuoz-st-moritz-sprint",
        "marcialonga",
        "bedrichov-sprint",
        "jizerska-padesatka",
        "oxberg-mora-sprint-women",
        "oxberg-mora-sprint-men",
        "vasaloppet",
        "birkebeinerrennet",
        "reistadlopet",
        "summit-2-senja"
    ];

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
    var leaderName as String = "";
    var projectedDiff as String = "";
    var isOffline as Boolean = false;
    var lastRequestUrl as String = "";

    // Set the label of the data field here.
    function initialize() {
        DataField.initialize();
    }

    function getRaceSlug() as String {
        var raceSetting = Application.Properties.getValue("race");

        if (raceSetting instanceof Number) {
            var raceIndex = raceSetting;
            if (raceIndex >= 0 && raceIndex < RACE_SLUGS.size()) {
                return RACE_SLUGS[raceIndex];
            }
        } else if (raceSetting instanceof String) {
            if (RACE_SLUGS.indexOf(raceSetting) >= 0) {
                return raceSetting;
            }
        }

        return "vasaloppet";
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
        var race = getRaceSlug();
        var dryRun = Application.Properties.getValue("dryRun");
        var medalTimePct = Application.Properties.getValue("medalTimePct");      
        
            // Get elapsed time in minutes (milliseconds to minutes)
            var elapsedMinutes = 0.0;
            if (info.elapsedTime != null) {
                elapsedMinutes = info.elapsedTime / 60000.0;
            }

            if (elapsedMinutes == 0.000000){
                elapsedMinutes = 0.000001;
            }            

            var url = "https://vasalivefeeder.azurewebsites.net/api/TempoDelta?race=" + race + "&km=" + distanceKm + "&speed=" + speed + "&elapsed=" + elapsedMinutes + "&dryRun=" + dryRun + "&medalTimePct=" + medalTimePct;
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
                    currentDelta = "ERR:NoSpeed";
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
                var leaderNameValue = data.get("leaderName");
                if (leaderNameValue != null && leaderNameValue instanceof String) {
                    leaderName = leaderNameValue;
                } else {
                    leaderName = "";
                }
                var projectedDiffValue = data.get("projectedDiff");
                if (projectedDiffValue != null && projectedDiffValue instanceof String) {
                    projectedDiff = projectedDiffValue;
                } else {
                    projectedDiff = "";
                }
                isOffline = false;
            } else {
                currentDelta = "ERR:NoDict";
                leaderDistanceKm = 0.0;
                leaderName = "";
                projectedDiff = "";
                isOffline = true;
            }
        } else {
            if (responseCode == 404) {
                currentDelta = "No race";
                isOffline = false;
            } else {
                currentDelta = "ERR:" + responseCode;
                isOffline = true;
            }
            leaderDistanceKm = 0.0;
            leaderName = "";
            projectedDiff = "";
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
        var tempoFont = Graphics.FONT_LARGE;
        // Draw tempoText centered as before
        dc.setColor(fgColor, Graphics.COLOR_TRANSPARENT);
        dc.drawText(w/2, tempoY, tempoFont, tempoText, Graphics.TEXT_JUSTIFY_CENTER);
        var unitText = "min/km";
        var tempoWidth = dc.getTextWidthInPixels(tempoText, tempoFont);
        var unitX = w/2 + tempoWidth/2 + 8;
        if (!healthStatusOk) {
            dc.setColor(Graphics.COLOR_RED, Graphics.COLOR_TRANSPARENT);
        } else {
            dc.setColor(fgColor, Graphics.COLOR_TRANSPARENT);
        }
        dc.drawText(unitX, tempoY, tempoFont, unitText, Graphics.TEXT_JUSTIFY_LEFT);

        var indicatorFont = Graphics.FONT_XTINY;
        var indicatorText = "";
        var indicatorColor = fgColor;
        if (isOffline) {
            indicatorText = "OFFLINE";
            indicatorColor = Graphics.COLOR_RED;
        } else if (projectedDiff.length() > 0) {
            indicatorText = projectedDiff;
            if (projectedDiff.find("-") == 0) {
                indicatorColor = Graphics.COLOR_ORANGE;
            } else if (projectedDiff.find("+") == 0) {
                indicatorColor = Graphics.COLOR_GREEN;
            }
        }
        if (indicatorText.length() > 0) {
            dc.setColor(indicatorColor, Graphics.COLOR_TRANSPARENT);
            dc.drawText(w/2, tempoY - 20, indicatorFont, indicatorText, Graphics.TEXT_JUSTIFY_CENTER);
        }

        // Draw leader distance (small font) further below
        var raceDistanceKm = getRaceDistanceKm();
        var leaderLabel = "Leader";
        if (leaderDistanceKm >= raceDistanceKm) {
            leaderLabel = "Finished";
        }

        var showLeaderName = leaderName.length() > 0 && ((Time.now().value() % 10) >= 5);
        var leaderText = leaderLabel + ": -- km";
        if (showLeaderName) {
            leaderText = leaderLabel + ": " + leaderName;
        } else if (leaderDistanceKm >= 0) {
            leaderText = leaderLabel + ": " + leaderDistanceKm.format("%.2f") + " km";
        } else if (leaderDistanceKm < 0) {
            leaderText = "Race Complete";
        }
        var leaderY = tempoY + 55;
        dc.setColor(fgColor, Graphics.COLOR_TRANSPARENT);
        dc.drawText(w/2, leaderY, Graphics.FONT_SMALL, leaderText, Graphics.TEXT_JUSTIFY_CENTER);

        // In dry run mode, show active settings for verification
        var dryRun = Application.Properties.getValue("dryRun");
        if (dryRun instanceof Boolean && dryRun) {
            var race = getRaceSlug();
            var medalTimePct = Application.Properties.getValue("medalTimePct");
            var debugText = race + " /" + medalTimePct + "%";
            dc.setColor(Graphics.COLOR_DK_GRAY, Graphics.COLOR_TRANSPARENT);
            dc.drawText(w/2, leaderY + 22, Graphics.FONT_XTINY, debugText, Graphics.TEXT_JUSTIFY_CENTER);
        }
    }

    function getRaceDistanceKm() as Float {
        var race = getRaceSlug();

        if (race == "bad-gastein-prologue") {
            return 10.0;
        } else if (race == "sportgastein-criterium") {
            return 20.0;
        } else if (race == "bad-gastein-itt-2") {
            return 10.0;
        } else if (race == "bad-gastein-criterium") {
            return 20.0;
        } else if (race == "engadin-la-diagonela") {
            return 47.0;
        } else if (race == "zuoz-st-moritz-sprint") {
            return 10.0;
        } else if (race == "marcialonga") {
            return 70.0;
        } else if (race == "bedrichov-sprint") {
            return 10.0;
        } else if (race == "jizerska-padesatka") {
            return 50.0;
        } else if (race == "oxberg-mora-sprint-women") {
            return 10.0;
        } else if (race == "oxberg-mora-sprint-men") {
            return 10.0;
        } else if (race == "vasaloppet") {
            return 90.0;
        } else if (race == "birkebeinerrennet") {
            return 53.0;
        } else if (race == "reistadlopet") {
            return 60.0;
        } else if (race == "summit-2-senja") {
            return 40.0;
        }

        return 40.0;
    }

}