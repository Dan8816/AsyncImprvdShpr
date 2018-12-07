//This script is to prevent scroll reset after an async callback using update panels with Web forms --Dan Engle 12/7/2018
var xPos, yPos;
Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(BeginRequestHandler);
Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(EndRequestHandler);
function BeginRequestHandler(sender, args) {
    xPos = document.body.scrollLeft;
    yPos = document.body.scrollTop;
}
function EndRequestHandler(sender, args) {
    document.body.scrollLeft = xPos;
    document.body.scrollTop = yPos;
}
//Below code works but would need ot use a class for multiple update panels Dan Engle 12/7/2018
    //var xPos, yPos;
    //var prm = Sys.WebForms.PageRequestManager.getInstance();
    //prm.add_beginRequest(BeginRequestHandler);
    //prm.add_endRequest(EndRequestHandler);
    //function BeginRequestHandler(sender, args) {
    //    xPos = $get('#ddlCountries').scrollLeft;
    //    yPos = $get('#ddlCountries').scrollTop;
    //}
    //function EndRequestHandler(sender, args) {
    //    $get('#ddlCountries').scrollLeft = xPos;
    //    $get('#ddlCountries').scrollTop = yPos;
    //}