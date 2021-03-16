# Frida Android Tips

Some notes to easy things up for me that may help you too.

## Setup

### Install
```bash
$ pip install frida-tools
```

### Hooking USB Device

You could run "frida-ps -U" to confirm if the device is registered as Gadget. 

```bash
$ frida -U Gadget -l .\frida\script.js --no-pause
```

### ADB Forward connection

Had some trouble connecting at the frida server and this solved my issue. If you are running things with root probably won't need this.

```bash
$ adb forward tcp:27042 tcp:27042
```

## Scripts

Simple script to hook class method, print output and return it to the application:

```javascript
if(Java.available){
	 console.log("Java is available");
	 setTimeout(function(){
		 Java.perform(function () {
			 var IM = Java.use("CLASS");
			 IM.FUNCTION.overload().implementation = function () {
			 send("Inside - function getNumEstabelecimento()");
			 var ret = this.getNumEstabelecimento();
			 send("Result:" + ret);
			 return ret;
		  };
		 });
	 },0);
}
```

Sample of reflection using python, prepared to be used with device:

```python
import frida
import sys

package_name = "Gadget"

def get_messages_from_js(message, data):
	print(message)
	print (message['payload'])


def instrument_load_url():
	hook_code = """
		JS
	"""
	return hook_code

process = frida.get_device_manager().enumerate_devices()[-1].attach(package_name)
script = process.create_script(instrument_load_url())
script.on('message',get_messages_from_js)
script.load()

sys.stdin.read()
```

Method overload example:

```javascript
 var Log = Java.use("br.com.app.log.AndroidLog");
 Log.d.overload("java.lang.String").implementation = function (a) {
	 send(a.toString());
	 return;
 };
 ```
