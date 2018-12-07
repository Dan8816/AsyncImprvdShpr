<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Completed.aspx.cs" Inherits="ImprovedShipper.Completed" %>
<%@ PreviousPageType VirtualPath="~/default.aspx" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <!-- <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>-->
    <!--<link href="~/Styles/Site.css" rel="stylesheet" type="text/css" />original css link-->
    <!--This page was refactored to a modernized layout that may display for mobile devices by Dan Engle 10/5/2018 but 
        the tags with "asp" attribute were left-over from exisitng code-->
    <title>Shipping Request Submitted</title>
    <link href="Content/bootstrap.css" rel="stylesheet" />
</head>
<body>
    <div class="container">
        <nav class="navbar navbar-dark bg-dark" style="padding: 0px;">
            <div class="container-fluid">
                <div class="navbar-header">
                    <a class="navbar-brand" href="https://www.oai.aero/"><img src="images/omni_logo.png" alt="" style="padding: 0px; margin: 0px;"></a>
                    <button class="btn btn-sm btn-primary" type="button"><asp:hyperlink ID="Hyperlink1" runat="server" NavigateUrl="~/default.aspx" style="color:azure;">Back to Request Page</asp:hyperlink></button>
                </div>
            </div>
        </nav>
    <div class="header text-center">
        <h4 class="display-5" style="color: Maroon">OAI Shipping Request</h4>
        <h5>Valid shipping request has been submitted! If purpose is business, your manager will receive a summary page of this information. Please print this page to send with your shipment.</h5>
    </div>
    <form class="form" id="form1" runat="server">
        
        <hr>
            <asp:Label ID="lblCurrentDate" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblShipID" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblShipType" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblInitiatingDepartment" runat="server" Text=""></asp:Label><br /><br />
        <hr>
            <asp:Label ID="lblFromName" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblFromPhone" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblFromEmail" runat="server" Text=""></asp:Label><br /><br />
        <hr>
            <asp:Label ID="lblToAttn" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblToCountry" runat="server" Text=""></asp:Label><br />
     
            <asp:Label ID="lbltoCompanyOrNameOrName" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblAddressOne" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblAddressTwo" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblToCity" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblToState" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblToPostalCode" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblToPhone" runat="server" Text=""></asp:Label><br /><br />
        <hr>
            <asp:GridView ID="gvItems" runat="server"></asp:GridView><br />
        <hr>
            <asp:Label ID="lblShipDate" runat="server" Text=""></asp:Label><br />

            <asp:Label ID="lblShipTime" runat="server" Text=""></asp:Label><br /><br />
        <hr>
            <asp:Label ID="lblUrgency" runat="server" Text=""></asp:Label><br /><br />

            <asp:Label ID="lblComments" runat="server" Text=""></asp:Label><br /><br />

            <asp:Label ID="lblBillToThirdParty" runat="server" Text=""></asp:Label><br /><br />

            <asp:Label ID="lblSignatureRequired" runat="server" Text=""></asp:Label><br /><br />

            <asp:Label ID="Label2" runat="server" Text="Note: External debts related to a personal pupose shipping request will be processed as a personal payroll deduction"></asp:Label><br />
        <br />
        </form>
    </div>
</body>
</html>
