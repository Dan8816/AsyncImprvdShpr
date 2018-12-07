<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Retrieve.aspx.cs" Inherits="ImprovedShipper.Retrieve" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Retrieve a Reuest</title>
    <link href="Content/bootstrap.css" rel="stylesheet" />
    <link rel="stylesheet" type="text/css" href="/DataTables/datatables.css" />
    <script src="Scripts/jquery-3.3.1.js"></script>
    <script type="text/javascript" charset="utf8" src="/DataTables/datatables.js"></script>
</head>
<body>
    <div class="container">
    <nav class="navbar navbar-dark bg-dark" style="padding: 0px;">
        <div class="container-fluid">
            <div class="navbar-header">
                <a class="navbar-brand" href="https://www.oai.aero/"><img src="images/omni_logo.png" alt="" style="padding: 0px; margin: 0px;"></a>
                <button class="btn btn-sm btn-primary" type="button"><a style="color:azure;" href="default.aspx">Make new Ship Request</a></button>
            </div>
        </div>
    </nav>
    <div class="header text-center">
        <h4 class="display-5">Retrieve Shipping Request Information</h4>
    </div>
    <form class="form" id="form1" runat="server">
        <asp:Literal ID="Literal1" runat="server"></asp:Literal>
        <!--<asp:GridView ID="GridView1" runat="server" class="display table table-hover text-center"></asp:GridView>-->
    </form>
    </div>
</body>
<script>
    $(document).ready(function () {
        $('#main-table').DataTable();
    });
</script>
</html>

