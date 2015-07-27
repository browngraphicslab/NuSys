console.log("ADDED");

var allUrls=null;
var allData=null;


chrome.storage.local.get(null, function (data) { 
	console.log(data);
	allData = data;
	delete allData["curr"];
	var date = allData["selectionTime"];
 //   console.log(allData.l)
	allUrls = Object.keys(allData);


	console.log(allUrls);
	setData();
});

function setData(){
$.each(allUrls, function(index, url){
	console.log(url);
	var div = document.createElement("div");
	div.setAttribute("id", index);
	document.body.appendChild(div);
	$("#"+index).append("<p style='font-size:160%' >"+url+"</p>");
	console.log(allData[url]);
	$.each(allData[url], function(indx, val){
		var res = val.split('//');
		var newVal = "";
		for (var i=0; i<res.length; i++){
			newVal += res[i];
			if (i<res.length-1 && res[i][res[i].length-1]!=":"){
				newVal += "https://";
			}
		}
		$("#"+index).append("<div style = 'clear:both'>"+newVal+"</div>");
	})

	$("#"+index).append("<hr>");
	document.getElementById("container").appendChild(div);

})
}

