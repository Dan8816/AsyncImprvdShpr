using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Net.Mail;
using System.DirectoryServices;
using System.Security.Principal;

namespace ImprovedShipper
{
    public partial class Completed : System.Web.UI.Page
    {
        //>>===============================================================================added by Dan Engle 10/11/19==================================================================================>>
        //string path = "LDAP://omni.phish";//"LDAP://oai.local/OU=Users,OU=OAI Company,dc=oai,dc=local";
        //string path = "LDAP://" + ConfigurationManager.AppSettings["omni.phish"] + "/dc=" + ConfigurationManager.AppSettings["sld"] + ",dc=" + ConfigurationManager.AppSettings["tld"];
        //string username = "sa-shipreq-01";
        //string password = "p@$$word999999";
        //public string GetUsersManager(string samaccount)
        //{
        //    System.Diagnostics.Debug.WriteLine("***** Successfully called the GetUsrMgr func and param val is: " + samaccount + "*****");
        //    string UsersManager = "";
        //    try
        //    {
        //        string ThisUsersName = samaccount;
        //        string Userfilter = "(&(objectCategory=person)(objectClass=user)(sAMAccountName=" + ThisUsersName + ")(!userAccountControl:1.2.840.113556.1.4.803:=2))";
        //        System.Diagnostics.Debug.WriteLine("Userfilter is: " + Userfilter);
        //        string[] UsersProperties = new string[1] { "manager" };
        //        System.Diagnostics.Debug.WriteLine("username: " + username + " password: " + password);
        //        DirectoryEntry UserEntry = new DirectoryEntry(path, username, password);
        //        DirectorySearcher UserSearch = new DirectorySearcher(UserEntry, Userfilter, UsersProperties);
        //        SearchResultCollection UserResults = UserSearch.FindAll();
        //        foreach (SearchResult result in UserResults)
        //        {
        //            UsersManager = (string)result.Properties["manager"][0];
        //            System.Diagnostics.Debug.WriteLine("***** Manager Value is: " + UsersManager + "*****");
        //        }
        //    }
        //    catch { }
        //    return UsersManager;
        //}
        //public string GetManagersData(string MgrDN)
        //{
        //    System.Diagnostics.Debug.WriteLine("***** Successfully called the GetManagersData func and param val is: " + MgrDN + "*****");
        //    string ManagerData = "";
        //    try
        //    {
        //        string UserManager = MgrDN;
        //        string Managerfilter = "(&(objectCategory=person)(objectClass=user)(DistinguishedName=" + UserManager + ")(!userAccountControl:1.2.840.113556.1.4.803:=2))";
        //        string[] ManagerProperties = new string[1] { "mail" };
        //        DirectoryEntry ManagerEntry = new DirectoryEntry(path, username, password);
        //        DirectorySearcher ManagerSearch = new DirectorySearcher(ManagerEntry, Managerfilter, ManagerProperties);
        //        SearchResultCollection ManagerResults = ManagerSearch.FindAll();
        //        foreach (SearchResult result in ManagerResults)
        //        {
        //            ManagerData = (string)result.Properties["mail"][0];
        //            System.Diagnostics.Debug.WriteLine("***** ManagerEmail Value is: " + ManagerData + "*****");
        //        }
        //    }
        //    catch { }
        //    return ManagerData;
        //}
        string SenderName = String.Empty;
        string ReceiverName = String.Empty;
        string ReceiverAdd = String.Empty;
        string ReceiverCity = String.Empty;
        string ReceiverCountry = String.Empty;
        string ShipRequestID = String.Empty;
        string ThereByDate = String.Empty;

        //<<===========================================================================added by Dan Engle 10/11/2018=================================================================================<<
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)//this code runs once when page initially loads but not after refresh or post request
            {
                var shippingDate = PreviousPage.FindControl("txtTodaysDate") as TextBox;
                if (shippingDate != null)
                {
                    lblCurrentDate.Text = "<b>Date:</b> " + shippingDate.Text;
                }
                var shipID = PreviousPage.FindControl("txtShipID") as TextBox;
                if (shipID != null)
                {
                    lblShipID.Text = "<b>Ship #:</b> " + shipID.Text;
                }
                var shipType = PreviousPage.FindControl("rblBusinessPrivate") as RadioButtonList;
                if (shipType != null)
                {
                    if (shipType.Text == "B")
                    {
                        lblShipType.Text = "<b>Ship Type:</b> Business";
                    }
                    else
                    {
                        lblShipType.Text = "<b>Ship Type:</b> Personal";
                    }
                }
                lblInitiatingDepartment.Text = "<b>Initiating Department:</b> " + GetInitiatingDepartment();

                var fromName = PreviousPage.FindControl("txtFromName") as TextBox;
                if (fromName != null)
                {
                    lblFromName.Text = "<b>FROM:</b> " + fromName.Text;
                    //SenderName = fromName.Text;//set the outside scope variable to the text attribute contained in the object called fromName
                    //System.Diagnostics.Debug.WriteLine("*****SenderName is :" + SenderName + "*****");//this verified the value
                }
                var fromPhone = PreviousPage.FindControl("txtFromPhone") as TextBox;
                if (fromPhone != null)
                {
                    lblFromPhone.Text = "<b>Phone:</b> " + fromPhone.Text;

                }
                var fromEmail = PreviousPage.FindControl("txtEmailAddress") as TextBox;
                if (fromEmail != null)
                {
                    lblFromEmail.Text = "<b>Email:</b> " + fromEmail.Text;
                }
                var toAttn = PreviousPage.FindControl("txtToAttn") as TextBox;
                if (toAttn != null)// page validation prevents this form being false Dan Engle 11/02/2018
                {
                    lblToAttn.Text = "<b>TO:</b> " + toAttn.Text;
                    System.Diagnostics.Debug.WriteLine(">>>>>toAttn is " + toAttn.Text + " <<<<<");
                }

                var toCountry = PreviousPage.FindControl("ddlCountries") as DropDownList;
                if (toCountry != null)
                {
                    lblToCountry.Text = "<b>Country:</b> " + toCountry.Text;
                }
                var toCompanyOrName = PreviousPage.FindControl("txttoCompanyOrNameOrName") as TextBox;
                if (toCompanyOrName != null)
                {

                    System.Diagnostics.Debug.WriteLine(">>>>>toCompanyOrName is " + toCompanyOrName.Text + " <<<<<");
                    if (toAttn.Text == "")
                    {
                        lblToAttn.Text = "<b>To:</b> " + toCompanyOrName.Text;
                        lbltoCompanyOrNameOrName.Text = "<b>Company:</b> " + toAttn.Text;
                    }
                    else
                    {
                        lbltoCompanyOrNameOrName.Text = "<b>Company:</b> " + toCompanyOrName.Text;
                    }
                }

                var toAddressOne = PreviousPage.FindControl("txtToAddressOne") as TextBox;
                if (toAddressOne != null)
                {
                    lblAddressOne.Text = "<b>Address One:</b> " + toAddressOne.Text;
                }
                var toAddressTwo = PreviousPage.FindControl("txtToAddressTwo") as TextBox;
                if (toAddressTwo != null)
                {
                    lblAddressTwo.Text = "<b>Address Two:</b> " + toAddressTwo.Text;
                }
                var city = PreviousPage.FindControl("txtCity") as TextBox;
                if (city != null)
                {
                    lblToCity.Text = "<b>City:</b> " + city.Text;
                }

                //CCalk 9/18/2012 If not US then do not display state
                var state = PreviousPage.FindControl("ddlStates") as DropDownList;

                if (toCountry.Text != "US") //The country is NOT US so do not show a state
                {
                    lblToState.Text = "<b>State:</b> ";
                }
                else
                {
                    if (state != null)
                    {
                        lblToState.Text = "<b>State:</b> " + state.Text;
                    }
                }
                var postalCode = PreviousPage.FindControl("txtPostalCode") as TextBox;
                if (postalCode != null)
                {
                    lblToPostalCode.Text = "<b>Postal Code:</b> " + postalCode.Text;
                }
                var phone = PreviousPage.FindControl("txtToPhone") as TextBox;
                if (phone != null)
                {
                    lblToPhone.Text = "<b>Phone:</b> " + phone.Text;
                }
                //Shipping items list
                GetShippingItems(Convert.ToInt16(shipID.Text));

                var shipDate = PreviousPage.FindControl("calShipByDate") as Calendar;
                if (shipDate != null)
                {
                    lblShipDate.Text = "<b>Date:</b> " + shipDate.SelectedDate.ToShortDateString();
                }
                var shipTime = PreviousPage.FindControl("ddlShipByTime") as DropDownList;
                if (shipTime != null)
                {
                    lblShipTime.Text = "<b>Time:</b> " + shipTime.Text;
                }
                //added by Dan 10/29/2018
                var urgency = PreviousPage.FindControl("ddlUrgency") as DropDownList;
                if (urgency != null)
                {
                    lblUrgency.Text = "<b>Urgency:</b> " + urgency.Text;
                }

                var comments = PreviousPage.FindControl("txtComments") as TextBox;
                if (comments != null)
                {
                    lblComments.Text = "<b>Comments:</b> " + comments.Text;
                }
                var billToThirdParty = PreviousPage.FindControl("txtBillTo3rdParty") as TextBox;
                if (billToThirdParty != null)
                {
                    lblBillToThirdParty.Text = "<b>Bill to 3rd Party:</b> " + billToThirdParty.Text;
                }
                var signatureRequired = PreviousPage.FindControl("cbSignatureRequired") as CheckBox;
                if (signatureRequired != null)
                {
                    lblSignatureRequired.Text = "<b>Signature Required:</b> " + signatureRequired.Checked;
                }
            }
            else
            {
                SenderName = lblFromName.Text.Substring(13);
                ReceiverName = lbltoCompanyOrNameOrName.Text.Substring(16);
                ReceiverAdd = lblAddressOne.Text.Substring(20);
                ReceiverCity = lblToCity.Text.Substring(13);
                ReceiverCountry = lblToCountry.Text.Substring(16);
                ShipRequestID = lblShipID.Text.Substring(15);
                ThereByDate = lblShipDate.Text.Substring(13);
            }

        }
        private void GetShippingItems(int id)
        {
            SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString);
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = String.Format(@"SELECT description, quantity FROM Shipping_Items WHERE ID = {0}", id);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = myConnection;

            DataSet myDataSet = new DataSet();

            SqlDataAdapter myAdapter = new SqlDataAdapter(cmd);
            myAdapter.Fill(myDataSet);

            //Bind the DataSet to the GridView
            gvItems.DataSource = myDataSet;
            gvItems.DataBind();

            //Close the connection
            myConnection.Close();
        }
        string GetInitiatingDepartment()
        {
            string initiatingDepartment = "";

            var initiatingDepartmentPrimary = PreviousPage.FindControl("ddlInitiatingDepartment") as DropDownList;
            var initiatingDepartmentSub = PreviousPage.FindControl("ddlSubDepartments") as DropDownList;
            var initiatingDepartmentOther = PreviousPage.FindControl("txtOtherInitiatingDepartment") as TextBox;

            string strInitiatingDepartmentPrimary = GetInitiatingDepartmentPrimary(Convert.ToInt16(initiatingDepartmentPrimary.Text));

            if (strInitiatingDepartmentPrimary == "Administration" || strInitiatingDepartmentPrimary == "MX" || strInitiatingDepartmentPrimary == "Operations")
                initiatingDepartment = GetInitiatingDepartmentSub(Convert.ToInt16(initiatingDepartmentSub.Text));
            else if (initiatingDepartmentPrimary.Text == "Other")
                initiatingDepartment = initiatingDepartmentOther.Text;
            else
                initiatingDepartment = strInitiatingDepartmentPrimary;

            return initiatingDepartment;
        }
        private string GetInitiatingDepartmentSub(int sd)
        {
            SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString);
            SqlCommand cmd = new SqlCommand();
            Object returnValue;

            cmd.CommandText = String.Format(@"SELECT Department_Name FROM Shipping_Department WHERE Department_ID = {0}", sd);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = myConnection;

            myConnection.Open();

            returnValue = cmd.ExecuteScalar();

            myConnection.Close();

            return returnValue.ToString();
        }
        private string GetInitiatingDepartmentPrimary(int pd)
        {
            //CCALK Pull initiating department from DB

            SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString);
            SqlCommand cmd = new SqlCommand();
            Object returnValue;

            cmd.CommandText = String.Format(@"SELECT Department_Name FROM Shipping_Department WHERE Department_ID = {0}", pd);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = myConnection;

            myConnection.Open();

            returnValue = cmd.ExecuteScalar();

            myConnection.Close();

            return returnValue.ToString();
        }
    }
}