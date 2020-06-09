mergeInto(LibraryManager.library, {

	GetCSRFToken: function () {
		var name = "csrftoken" + "=";
		var decodedCookie = decodeURIComponent(document.cookie);
		var ca = decodedCookie.split(';');
		var returnStr = "";
		for(var i = 0; i <ca.length; i++) {
			var c = ca[i];			
			while (c.charAt(0) == ' ') {
				c = c.substring(1);
			}
			if (c.indexOf(name) == 0) {
				returnStr = c.substring(name.length, c.length);
				var bufferSize = lengthBytesUTF8(returnStr) + 1;
				var buffer = _malloc(bufferSize);
				stringToUTF8(returnStr, buffer, bufferSize);
				return buffer;
			}
		}
		return returnStr;
	},

	GetUserName: function () {
		var name = "username" + "=";
		var decodedCookie = decodeURIComponent(document.cookie);
		var ca = decodedCookie.split(';');
		var returnStr = "";
		for(var i = 0; i <ca.length; i++) {
			var c = ca[i];			
			while (c.charAt(0) == ' ') {
				c = c.substring(1);
			}
			if (c.indexOf(name) == 0) {
				returnStr = c.substring(name.length, c.length);
				var bufferSize = lengthBytesUTF8(returnStr) + 1;
				var buffer = _malloc(bufferSize);
				stringToUTF8(returnStr, buffer, bufferSize);
				return buffer;
			}
		}
		return returnStr;
	},
	
	GetIsAI: function () {
		var name = "use_ai" + "=";
		var decodedCookie = decodeURIComponent(document.cookie);
		var ca = decodedCookie.split(';');
		var returnStr = "";
		for(var i = 0; i <ca.length; i++) {
			var c = ca[i];			
			while (c.charAt(0) == ' ') {
				c = c.substring(1);
			}
			if (c.indexOf(name) == 0) {
				returnStr = c.substring(name.length, c.length);
				var bufferSize = lengthBytesUTF8(returnStr) + 1;
				var buffer = _malloc(bufferSize);
				stringToUTF8(returnStr, buffer, bufferSize);
				return buffer;
			}
		}
		return returnStr;
	},

});
