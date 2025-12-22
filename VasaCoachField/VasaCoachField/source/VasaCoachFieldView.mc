import Toybox.Activity;
import Toybox.Communications;
import Toybox.Lang;
import Toybox.Time;
import Toybox.WatchUi;

class VasaCoachFieldView extends WatchUi.SimpleDataField {

    // Set the label of the data field here.
    function initialize() {
        SimpleDataField.initialize();
        label = "My Label";
        fetchDataFromServer();
    }
    
    var currentDelta as Float = 0.0;
    var lastFetchTime as Number = 0;

    // The given info object contains all the current workout
    // information. Calculate a value and return it in this method.
    // Note that compute() and onUpdate() are asynchronous, and there is no
    // guarantee that compute() will be called before onUpdate().
    function compute(info as Activity.Info) as Numeric or Duration or String or Null {
        label = "Test";
        
        // Fetch data every 5 seconds
        var currentTime = Time.now().value();
        if (currentTime - lastFetchTime > 5) {
            fetchDataFromServer();
            lastFetchTime = currentTime;
        }

        // See Activity.Info in the documentation for available information.
        return currentDelta;
    }
    
    function fetchDataFromServer() as Void {
        var url = "http://192.168.0.29:7140/api/TempoDelta?value=2.23&code=<FUNCTION_KEY>";
        var options = {
            :method => Communications.HTTP_REQUEST_METHOD_GET
        };
        
        Communications.makeWebRequest(url, null, options, method(:onReceive));
    }
    
    function onReceive(responseCode as Number, data as Dictionary or String or Null) as Void {
        if (responseCode == 200 && data != null) {
            // Try to parse as a simple number
            if (data instanceof Number) {
                currentDelta = data.toFloat();
            } else if (data instanceof Float) {
                currentDelta = data;
            } else if (data instanceof String) {
                try {
                    currentDelta = data.toFloat();
                } catch (ex) {
                    currentDelta = -9999.0; // Parse error
                }
            } else {
                currentDelta = -7777.0; // Unexpected data type
            }
        } else {
            // Show error code
            currentDelta = responseCode.toFloat();
        }
    }   

}