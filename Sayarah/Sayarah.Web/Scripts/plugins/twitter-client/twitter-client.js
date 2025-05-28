(function (window) {

	var api_key, oauth_token, request_url, popup;
	var authorize_url = 'https://api.twitter.com/oauth/authenticate?oauth_token=';

	function init(options) {
		api_key = options.api_key;
		request_url = options.request_url;
	}

	function closePopup() {
		if (popup && !popup.closed) {
			popup.close();
		}
	}

    function getUrlQueryObject(query_string) {
        
		var vars = {}, hash;
		if (!query_string) {
			return false;
		}
		var hashes = query_string.slice(1).split('&');
		for (var i = 0; i < hashes.length; i++) {
			hash = hashes[i].split('=');
			vars[hash[0]] = hash[1];
		}
		return vars;
	}

	function sendError(message, callback) {
		var response = {
			success: false,
			message: message || 'Some Error Occurred'
		};
		if (typeof callback === 'function') {
			callback(response);
		}
	}

	function getOAuthToken(callback) {
		var xhr = new XMLHttpRequest();
		xhr.onreadystatechange = function () {
			if (this.readyState == 4) {
				if (this.status === 0) {
					return callback('Internet Disconnected/Connection Timeout')
				}

				try {
					var response = JSON.parse(this.response);
					callback(null, response);
				} catch (error) {
					callback(error.message);
				}
				return;
			}
		};
		xhr.open("GET", request_url, true);
		xhr.send();
	}

    function authorize(callback) {
        
		if (!popup) {
			return callback('Popup Not initialized');
		}
		popup.location.href = authorize_url + oauth_token;
		var wait = function () {
			setTimeout(function () {
				return popup.closed ? callback(null, getUrlQueryObject(popup.location.search)) : wait();
			}, 25);
		};
		wait();
	}

    function connect(callback) {
        oauth_token ='Bs91Zw51Hg636bdlTHJmbOnGx'
		if (!request_url) {
			return sendError('Request URL not provided', callback);
		}
		//Open a blank popup
        popup = window.open(null, '_blank', 'height=400,width=800,left=250,top=100,resizable=yes', true);
        
        authorize(callback);
	}



    
	window.onload = function twitter() {
		if (typeof window.twitterInit == 'function') {
			window.twitterInit();
		}
	};

	window.twttr = {
		init: init,
		connect: connect
	};
})(window)