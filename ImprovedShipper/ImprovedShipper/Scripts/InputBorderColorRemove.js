//This script is to remove red border color of asp textbox or html input tags after setting them to red on a validations failing from server side method. The ajax call was working with the parameters but webform requied use of
//Webmethod which had to be a static method and could not simply access asp control elements after much unsuccessful attempts. The method will check for user changing input of a element belonging to a class, and remove then
//remove the border color style attribute after a 1000 milsec. after the last keyup. To adjust the response time change the last arg e.g. 1000 to another value in milliseconds
var timer;
$(".input-group").on("input", function (e) {//later I will use a class selector to avoid a handler for each input...1st get $(this) working
    var value = $(this).val();
    var color = $(this).css("border-color");
    var thisElement = $(this);
    //var data = { "id": thisElement.attr("id") };
    if ($(this).data("lastval") != value) {
        $(this).data("lastval", value);
        clearTimeout(timer);
        timer = setTimeout(function () {
            if (thisElement.css("border-color") == "rgb(255, 0, 0)") {
                //alert("hit the color is red bool");
                thisElement.removeAttr("style");
                //$.ajax({//jquery handles the following params very specifically   
                //    url: "default.aspx/SetBorderColors",//this specifies the method in code-behind
                //    data:'{"aspID":"' + thisElement.attr("id") + '"}',//aspId refers to the param name of this object in code-behind argument
                //    dataType: "json",//this specifies the response type
                //    type: "POST",//this specifies the HTTP method to server
                //    contentType: "application/json; charset=utf-8",//this specifies 
                //    success: function (response) {//takes the server response as a param
                //        console.log(response.d);
                //    },  
                //    failure: function () {  
                //         console.log("Error");  
                //    }  
                // });
            }
            else {
                console.log(value);
                console.log(color);
            }
        }, 1000);
    };
});