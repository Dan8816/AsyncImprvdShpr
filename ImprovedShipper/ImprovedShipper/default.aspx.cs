using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.Data.SqlClient;
using System.Data;
using System.Security.Principal;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.DirectoryServices;
using System.Net.Mail;
using ImprovedShipper.Models;
using System.Web.UI.HtmlControls;

namespace ImprovedShipper
{
    public partial class ShippingRequest : System.Web.UI.Page
    {
        //>===============================================> Dan Engle 10/8/2018 >=======================================================>

        SqlConnection myConnection;//instantiates a SqlConnection called myConnection, may need to encapsulate this
        public string path = "LDAP://" + ConfigurationManager.AppSettings["fqdn"] + "/dc=" + ConfigurationManager.AppSettings["sld"] +",dc=" + ConfigurationManager.AppSettings["tld"];
        public string username = ConfigurationManager.AppSettings["username"];//"sa-shippingrequest";
        public string password = ConfigurationManager.AppSettings["password"];//"7ho$pf$sPlnxng";
        public string GetUserEmpIDbySamAcct(string samaccount)
        {
            System.Diagnostics.Debug.WriteLine("***** Successfully called the GetUserEmpIDbySamAcct func and param val is: " + samaccount + " *****");
            string UsersEmpID = "";
            try
            {
                //string ThisUsersName = samaccount;
                string Userfilter = "(&(objectCategory=person)(objectClass=user)(sAMAccountName=" + samaccount + ")(!userAccountControl:1.2.840.113556.1.4.803:=2))";
                string[] UsersProperties = new string[1] { "employeeID" };
                //init a directory entry
                DirectoryEntry UserEntry = new DirectoryEntry(path, username, password);
                //init a directory searcher
                DirectorySearcher UserSearch = new DirectorySearcher(UserEntry, Userfilter, UsersProperties);
                SearchResultCollection UserResults = UserSearch.FindAll();
                foreach (SearchResult result in UserResults)
                {
                    UsersEmpID = (string)result.Properties["employeeID"][0];
                }
            }
            catch { }
            return UsersEmpID;
        }
        //public string UserSamName = HttpContext.Current.User.Identity.Name;//WindowsIdentity.GetCurrent().Name;//this get the current users full logged in name e.g. oai\dengle
        public string UsrEmpID = string.Empty;//will set this and record in db for retrieval later
        public bool ValidPage = true;//variable declared to use for data validation on submission
        public string connectionSTR = ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString;//"Server=AAG-Omni;Database=OAI_ShippingRequest;Integrated Security=SSPI";//MSSQL db string
        public string GetUsersManager(string samaccount)
        {
            System.Diagnostics.Debug.WriteLine("***** Successfully called the GetUsrMgr func and param val is: " + samaccount + "*****");
            string UsersManager = "";
            try
            {
            string ThisUsersName = samaccount;
            string Userfilter = "(&(objectCategory=person)(objectClass=user)(sAMAccountName=" + ThisUsersName + ")(!userAccountControl:1.2.840.113556.1.4.803:=2))";
            System.Diagnostics.Debug.WriteLine("Userfilter is: " + Userfilter);
            string[] UsersProperties = new string[1] { "manager" };
            System.Diagnostics.Debug.WriteLine("username: " + username + " password: " + password);
            DirectoryEntry UserEntry = new DirectoryEntry(path, username, password);
            DirectorySearcher UserSearch = new DirectorySearcher(UserEntry, Userfilter, UsersProperties);
            SearchResultCollection UserResults = UserSearch.FindAll();
            foreach (SearchResult result in UserResults)
            {
                UsersManager = (string)result.Properties["manager"][0];
                System.Diagnostics.Debug.WriteLine("***** Manager Value is: " + UsersManager + "*****");
            }
            }
            catch { }
            return UsersManager;
        }
        public string GetManagersData(string MgrDN)
        {
            System.Diagnostics.Debug.WriteLine("***** Successfully called the GetManagersData func and param val is: " + MgrDN + "*****");
            string ManagerData = "";
            try
            {
            string UserManager = MgrDN;
            string Managerfilter = "(&(objectCategory=person)(objectClass=user)(DistinguishedName=" + UserManager + ")(!userAccountControl:1.2.840.113556.1.4.803:=2))";
            string[] ManagerProperties = new string[1] { "mail" };
            DirectoryEntry ManagerEntry = new DirectoryEntry(path, username, password);
            DirectorySearcher ManagerSearch = new DirectorySearcher(ManagerEntry, Managerfilter, ManagerProperties);
            SearchResultCollection ManagerResults = ManagerSearch.FindAll();
            foreach (SearchResult result in ManagerResults)
            {
                ManagerData = (string)result.Properties["mail"][0];
                System.Diagnostics.Debug.WriteLine("***** ManagerEmail Value is: " + ManagerData + "*****");
            }
            }
            catch { }
            return ManagerData;
        }

        //[WebMethod]//this works but the webcontrols are an instance of the page and static is not an instance thus i can't access these inherited controls
        //public static string SetBorderColors(object aspID)
        //{
        //    string aspParam = aspID.ToString();
        //    System.Diagnostics.Debug.WriteLine("Successfully called SetBorderColors method and the param value is " + aspParam);
        //    //Page page = (Page)HttpContext.Current.Handler;
        //    //var thisElement = page.FindControl("txttoCompanyOrName") as TextBox;
        //    //if (thisElement == null)
        //    //{
        //    //    System.Diagnostics.Debug.WriteLine("thisElement is null");
        //    //}
        //    //else
        //    //{
        //    //    System.Diagnostics.Debug.WriteLine("Value is " + thisElement.Text);
        //    //}            
        //    //thisElement.BorderColor = System.Drawing.Color.Green;
        //    string JsonResponse = (new JavaScriptSerializer()).Serialize("The codebehind param is " + aspParam);
        //    return JsonResponse;
        //}

        //<==================================================< Dan Engle 10/9/2018 <===================================================<

        #region " Page Load "

        protected void Page_Load(object sender, EventArgs e)
        {
            //>===========================================> Dan Engle 10/8/2018 >=======================================================>
            txtTodaysDate.Text = DateTime.Now.ToShortDateString();          

            if (!IsPostBack)
            {
                string AccountName = HttpContext.Current.User.Identity.Name;// WindowsIdentity.GetCurrent().Name;//this get the current users full logged in name e.g. oai\dengle
                //System.Diagnostics.Debug.WriteLine("value of AccountName is " + AccountName);

                txtLoggedInUser.Text = AccountName;//this sets an existing var used on front-end to populate asp textbox called "txtLoggedInUser"
                                                   //and the var was used from CCALK's code but the assignment was changed

                UsrEmpID = GetUserEmpIDbySamAcct(AccountName.Substring(5));//(UserSamName);//
                //System.Diagnostics.Debug.WriteLine("***** EmpID Value is: " + UsrEmpID + "*****");

                GetISO3166_Countries();
                //<==============================================< Dan Engle 10/9/2018 <===========================================================<

                //CCALK 6/21/2012 WindowsIdentity would not work in our current setup - using Request.ServerVariables["LOGON_USER"] instead
                //txtLoggedInUser.Text = Request.ServerVariables["LOGON_USER"];//commented out by Dan Engle 10/8/2018

                //CCALK 6/12/2012 Pull departments from DB - populate department drop down list
                GetDepartments();

                //CCALK 6/12/2012 Set initial description text row for all drop down lists
                SetInitialDDLRow();

                //CCALK 6/15/2012 Get the user info from DB - populate Name, phone and email address fields
                GetUserInformation();
            }
        }
        // **********************************************   END Page Load   ************************************************************

        #endregion

        #region " Controls "

        //>>============================================= Start rblBusinessPrivate_SelectedIndexChanged ===============================>>

        protected void rblBusinessPrivate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rblBusinessPrivate.SelectedValue == "P")
            {
                PayDedRow.Visible = true;
            }
            else
            {
                PayDedRow.Visible = false;
            }
            ddlInitiatingDepartment.Focus();
        }
        //protected void ddlStates_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (ddlStates.BorderColor == System.Drawing.Color.Red)
        //    {
        //        ddlStates.BorderColor = System.Drawing.Color.Red;
        //    }
        //}

        // **********************************************   START ddlCountries_SelectedIndexChanged   **************************************
        //CCALK Disable Sates DDL if Contries DDL = anything other than "United States"
        protected void ddlCountries_SelectedIndexChanged(object sender, EventArgs e)
        {

            //System.Diagnostics.Debug.WriteLine("ddlCountries.SelectedValue == " + ddlCountries.SelectedValue);
            ddlStates.Items.Clear();
            if (ddlCountries.BorderColor == System.Drawing.Color.Red)
            {
                ddlCountries.BorderColor = System.Drawing.Color.Empty;
            }
            if (ddlCountries.SelectedValue == "US" || ddlCountries.SelectedValue == "MX")
            {
                //System.Diagnostics.Debug.WriteLine("Hit the state bool");
                ddlStates.Items.Insert(0, new ListItem("<State, district or other subunit is required>", "0"));
            }
            else if (ddlCountries.SelectedValue == "CA" || ddlCountries.SelectedValue == "AU" || ddlStates.SelectedValue == "CN")
            {
                //System.Diagnostics.Debug.WriteLine("Hit the province bool");
                ddlStates.Items.Insert(0, new ListItem("<Province, district or other subunit is required>", "0"));
            }
            else //if (ddlCountries.SelectedValue != "US" || ddlCountries.SelectedValue != "MX" || ddlCountries.SelectedValue != "CA" || ddlCountries.SelectedValue != "AU")
            {
                //System.Diagnostics.Debug.WriteLine("Hit the not required bool");
                ddlStates.Items.Insert(0, new ListItem("<Field may not be required>", "0"));
            }
            SqlConnection Subunit_List = new SqlConnection(connectionSTR);
            ShippingRequestDataContext db = new ShippingRequestDataContext(Subunit_List);
            var This_Nations_Subunits = from x in db.Political_Subunits
                                        where x.IATA_Code == ddlCountries.SelectedValue
                                        select new
                                        {
                                            Nation = x.IATA_Code,
                                            Unit = x.Political_Unit_Code,
                                            Name = x.Political_Unit_Name
                                        };
            int count = 1;
            foreach (var y in This_Nations_Subunits)
            {
                if (y.Unit.Length > 1)
                {
                    //System.Diagnostics.Debug.WriteLine("Found another subunit called " + y.Unit.Substring(3));
                    ddlStates.Items.Insert(count, new ListItem(y.Name, y.Unit.Substring(3)));
                    count++;
                    //System.Diagnostics.Debug.WriteLine("Value of count is " + count);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No unit code");
                }
            }
        }
        // **********************************************   END ddlCountries_SelectedIndexChanged   *********************************************************
        // **********************************************   START Clear Fields   *********************************************************
        //CCALK Clear all fields except logged in user, date, and the From fields (name, phone, email)
        protected void btnClear_Click(object sender, EventArgs e)
        {
            rblBusinessPrivate.SelectedIndex = 0;
            //lblShippingNumber.Text = string.Empty;
            ddlInitiatingDepartment.SelectedIndex = 0;
            //if (ddlSubDepartments.Visible == true) ddlSubDepartments.SelectedIndex = 0;//commented out by Dan Engle 11/28/2018 no longer need subdepts
            //ddlSubDepartments.Visible = false;
            txtOtherInitiatingDepartment.Text = string.Empty;
            txtOtherInitiatingDepartment.Visible = false;
            txtToAttn.Text = string.Empty;
            //ddlCountries.SelectedItem.Text = "United States";
            txttoCompanyOrName.Text = string.Empty;
            txtToAddressOne.Text = string.Empty;
            txtToAddressTwo.Text = string.Empty;
            txtCity.Text = string.Empty;
            ddlStates.Enabled = true;
            //ddlStates.SelectedItem.Text = "Alabama";
            txtPostalCode.Text = string.Empty;
            txtToPhone.Text = string.Empty;
            ddlNumberItemsShipped.SelectedIndex = 0;
            calShipByDate.SelectedDate = DateTime.Today;
            ddlShipByTime.SelectedIndex = 0;
            txtComments.Text = string.Empty;
            txtBillTo3rdParty.Text = string.Empty;
            cbSignatureRequired.Checked = false;
            txtDescriptionOne.Text = string.Empty;
            txtDescriptionTwo.Text = string.Empty;
            txtDescriptionThree.Text = string.Empty;
            txtDescriptionFour.Text = string.Empty;
            txtDescriptionFive.Text = string.Empty;
            txtDescriptionSix.Text = string.Empty;
            txtDescriptionOne.Visible = false;
            txtDescriptionTwo.Visible = false;
            txtDescriptionThree.Visible = false;
            txtDescriptionFour.Visible = false;
            txtDescriptionFive.Visible = false;
            txtDescriptionSix.Visible = false;
            txtQuantityOne.Text = string.Empty;
            txtQuantityTwo.Text = string.Empty;
            txtQuantityThree.Text = string.Empty;
            txtQuantityFour.Text = string.Empty;
            txtQuantityFive.Text = string.Empty;
            txtQuantitySix.Text = string.Empty;
            txtQuantityOne.Visible = false;
            txtQuantityTwo.Visible = false;
            txtQuantityThree.Visible = false;
            txtQuantityFour.Visible = false;
            txtQuantityFive.Visible = false;
            txtQuantitySix.Visible = false;
        }
        // **********************************************   END Clear Fields   *********************************************************
        // **********************************************   START Basic Focus Fields   *********************************************************
        protected void calShipByDate_SelectionChanged(object sender, EventArgs e)
        {
            if(calShipByDate.BorderColor == System.Drawing.Color.Red)
            {
                calShipByDate.BorderColor = System.Drawing.Color.Empty;
            }
            //ddlShipByTime.Focus();//this makes the front-end flicker worse but may help user to know the next step in the  form completion process 
        }
        protected void ddlShipByTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("hit the ddlShipByTime_SelectedIndexChanged method");
            if(ddlShipByTime.BorderColor == System.Drawing.Color.Red)
            {
                ddlShipByTime.BorderColor = System.Drawing.Color.Empty;
            }
        }
        // **********************************************   END Basic Focus Fields   *********************************************************
        // **********************************************   START ddlInitiatingDepartment_SelectedIndexChanged   *********************************************************
        //CCALK Logic to handle user selection:
        //  * Administration
        //  * Operations
        //  * MX
        //  * Other
        protected void ddlInitiatingDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlInitiatingDepartment.BorderColor == System.Drawing.Color.Red)
            {
                ddlInitiatingDepartment.BorderColor = System.Drawing.Color.Empty;
            }
            if (ddlInitiatingDepartment.SelectedItem.Text == "Other")
            {
                txtOtherInitiatingDepartment.Visible = true;
                txtOtherInitiatingDepartment.Focus();
            }
            else
            {
                txtOtherInitiatingDepartment.Visible = false;
            }  
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            //CCALK Verify required fields have been populated with data - if not a Validation summary StringBuilder is created and sent to the Validation Summary label
            ValidatePage();//This called the method to check validation of data Dan Engle 10/10/2018

            string SamName = HttpContext.Current.User.Identity.Name.Substring(5);//WindowsIdentity.GetCurrent().Name.Substring(5);//will need to change substr to 4 in prod//get the param to us as samaccount add Dan Engle 10/10/2018

            string Emp_ID = GetUserEmpIDbySamAcct(SamName);//add Dan Engle 10/10/2018

            //if (lblValidationSummary.Text.Length > 0) { return; }//Removed the validation to be checked based on the length of this is uses IsValid() Dan Engle 11/16/2018

            if (ValidPage == true)
            {
                //System.Diagnostics.Debug.WriteLine("Hit the IsValid == true bool and business purpose is " + rblBusinessPrivate.SelectedValue);
                //This is the bool to check if the purpose is Personal, the warehouse will get comments to send the user the shipping info Dan Engle 10/30/2018
                if (rblBusinessPrivate.SelectedValue == "P")
                {
                    txtComments.Text += @" ==>> This is a PERSONAL shipment and user shall need the cost and tracking info sent via email to " + txtEmailAddress.Text + " <<== ";
                }
                //This is to add the urgency to the comments requests by the warehouse for consideration of shipping cost options 10/29/2018
                txtComments.Text += @" ==>> Shipping urgency is " + ddlUrgency.SelectedItem.Text + " <<== ";//added by Dan Engle 10/30/2018 to put urgency in comments for warehouse        

                //if (txttoCompanyOrNameOrName.Text == "")//Due to changes in these front-end controls, html text, and event handlers for this controls, no longer need to flip these vals to meet UPS req
                //{
                //    txttoCompanyOrNameOrName.Text = txtToAttn.Text;
                //    txtToAttn.Text = "";
                //}

                using (SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString))
                {
                    myConnection.Open();
                    SqlCommand cmd = new SqlCommand();
                    //CCALK 6/6/2012 Insert Shipping Request
                    cmd = new SqlCommand("uspInsertShippingRequest", myConnection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Ship_Type_Business_Private", SqlDbType.NVarChar).Value = Server.HtmlDecode(rblBusinessPrivate.SelectedValue);
                    cmd.Parameters.Add("@ShipInitatingDate", SqlDbType.Date).Value = Convert.ToDateTime(txtTodaysDate.Text);
                    if (ddlInitiatingDepartment.SelectedItem.Text == "Other")//added by Dan Engle 11/28/2018 => this was to account for still using "Other" is ddl
                    {
                        cmd.Parameters.Add("@InitiatingDepartment", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtOtherInitiatingDepartment.Text);//this will record the value of txtOtherInitiatingDepartment.Text but it must be visible Dan Engle 11/28/2018
                    }
                    else
                    {
                        cmd.Parameters.Add("@InitiatingDepartment", SqlDbType.NVarChar).Value = Server.HtmlDecode(ddlInitiatingDepartment.SelectedItem.Text);
                    }
                    //END Determine if selected value is a primary, sub or "other" department and pass appropriate value to DB

                    cmd.Parameters.Add("@From_Name", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtFromName.Text);
                    cmd.Parameters.Add("@To_Name", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToAttn.Text);
                    cmd.Parameters.Add("@To_Company", SqlDbType.NVarChar).Value = Server.HtmlDecode(txttoCompanyOrName.Text);
                    cmd.Parameters.Add("@To_Address_One", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToAddressOne.Text);
                    cmd.Parameters.Add("@To_Address_Two", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToAddressTwo.Text);
                    cmd.Parameters.Add("@To_Phone", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToPhone.Text);
                    cmd.Parameters.Add("@From_Phone", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtFromPhone.Text);
                    cmd.Parameters.Add("@Shipped_By_Date", SqlDbType.Date).Value = Convert.ToDateTime(calShipByDate.SelectedDate);
                    cmd.Parameters.Add("@Shipped_By_Time", SqlDbType.Time).Value = ConvertTime(ddlShipByTime.SelectedValue);
                    cmd.Parameters.Add("@Signature_Required", SqlDbType.Bit).Value = cbSignatureRequired.Checked;
                    cmd.Parameters.Add("@Bill_To_Third_Party", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtBillTo3rdParty.Text);
                    cmd.Parameters.Add("@Logged_In_User", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtLoggedInUser.Text);
                    cmd.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtComments.Text);
                    cmd.Parameters.Add("@From_Email_Address", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtEmailAddress.Text);
                    cmd.Parameters.Add("@To_Country", SqlDbType.NVarChar).Value = Server.HtmlDecode(ddlCountries.SelectedItem.Value);
                    cmd.Parameters.Add("@To_City", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtCity.Text);
                    cmd.Parameters.Add("@Urgency", SqlDbType.NVarChar).Value = Server.HtmlDecode(ddlUrgency.SelectedItem.Value);//added by Dan 10/29/2018
                    cmd.Parameters.Add("@Employee_ID", SqlDbType.NVarChar).Value = Emp_ID;
                    cmd.Parameters.Add("@To_State", SqlDbType.NVarChar).Value = Server.HtmlDecode(ddlStates.SelectedItem.Value);
                    cmd.Parameters.Add("@To_Postal_Code", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtPostalCode.Text);
                    cmd.Parameters.Add("@PayDedConfirm", SqlDbType.Bit).Value = PayDedConfirm.Checked;
                    //CCALK 6/6/2012 Gets Id of inserted record 
                    object id;//declares an object called 'id'
                    id = cmd.ExecuteScalar();//sets the id equal to a sql execution to get 1st column of 1st record
                    txtShipID.Text = id.ToString();//populates the hidden input field with the result of the sql execution as a string
                                                   //CCALK 6/6/2012 Insert Items to be shipped
                    if (txtDescriptionOne.Text != "")
                    {
                        cmd = new SqlCommand("uspInsertShippingItems", myConnection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;//this sets the parameter of the stored procedure to the value if the shipID making the FK connected in the child table
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = txtDescriptionOne.Text;
                        cmd.Parameters.Add("@Quantity", SqlDbType.NVarChar).Value = txtQuantityOne.Text;
                        cmd.ExecuteNonQuery();
                    }
                    if (txtDescriptionTwo.Text != "")
                    {
                        cmd = new SqlCommand("uspInsertShippingItems", myConnection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;//this sets the parameter of the stored procedure to the value if the shipID making the FK connected in the child table
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = txtDescriptionTwo.Text;
                        cmd.Parameters.Add("@Quantity", SqlDbType.NVarChar).Value = txtQuantityTwo.Text;
                        cmd.ExecuteNonQuery();
                    }
                    if (txtDescriptionThree.Text != "")
                    {
                        cmd = new SqlCommand("uspInsertShippingItems", myConnection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;//this sets the parameter of the stored procedure to the value if the shipID making the FK connected in the child table
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = txtDescriptionThree.Text;
                        cmd.Parameters.Add("@Quantity", SqlDbType.NVarChar).Value = txtQuantityThree.Text;
                        cmd.ExecuteNonQuery();
                    }
                    if (txtDescriptionFour.Text != "")
                    {
                        cmd = new SqlCommand("uspInsertShippingItems", myConnection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;//this sets the parameter of the stored procedure to the value if the shipID making the FK connected in the child table
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = txtDescriptionFour.Text;
                        cmd.Parameters.Add("@Quantity", SqlDbType.NVarChar).Value = txtQuantityFour.Text;
                        cmd.ExecuteNonQuery();
                    }
                    if (txtDescriptionFive.Text != "")
                    {
                        cmd = new SqlCommand("uspInsertShippingItems", myConnection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;//this sets the parameter of the stored procedure to the value if the shipID making the FK connected in the child table
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = txtDescriptionFive.Text;
                        cmd.Parameters.Add("@Quantity", SqlDbType.NVarChar).Value = txtQuantityFive.Text;
                        cmd.ExecuteNonQuery();
                    }
                    if (txtDescriptionSix.Text != "")
                    {
                        cmd = new SqlCommand("uspInsertShippingItems", myConnection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;//this sets the parameter of the stored procedure to the value if the shipID making the FK connected in the child table
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = txtDescriptionSix.Text;
                        cmd.Parameters.Add("@Quantity", SqlDbType.NVarChar).Value = txtQuantitySix.Text;
                        cmd.ExecuteNonQuery();
                    }
                    /*
                    *CCALK 6/15/2012 Insert user information
                    * 1. Check to see if a row for that user exists - if it does not then create the row
                    * 2. If a row does exist for the user set the row to the data in the Name, Phone and Email fields
                    */
                    //1. Check to see if a row exists for this user
                    cmd = new SqlCommand("uspUserCount", myConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    System.Diagnostics.Debug.WriteLine("User is: " + txtLoggedInUser.Text);

                    cmd.Parameters.Add("@User_ID", SqlDbType.NVarChar).Value = txtLoggedInUser.Text;
                    object count = cmd.ExecuteScalar();
                    if (Convert.ToInt16(count) == 0)
                    //If count = 0 then no record exists for this user so create the record
                    {
                        cmd = new SqlCommand("uspInsertUserInformation", myConnection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@User_ID", SqlDbType.NVarChar).Value = txtLoggedInUser.Text;
                        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = txtFromName.Text;
                        cmd.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = txtFromPhone.Text;
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = txtEmailAddress.Text;
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        //count >= 1 the row exists - update existing row
                        cmd = new SqlCommand("uspUpdateUserInformation", myConnection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@User_ID", SqlDbType.NVarChar).Value = txtLoggedInUser.Text;
                        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = txtFromName.Text;
                        cmd.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = txtFromPhone.Text;
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = txtEmailAddress.Text;
                        cmd.ExecuteNonQuery();
                    }
                    //END Insert user information
                    myConnection.Close();//Left in place due to this connection being closed=>Dan Engle 10/16/2018
                                         //CCALK Create and display text file
                                         //CreateTextFile(id);
                }
                if (rblBusinessPrivate.SelectedValue == "B" && IsValid == true)
                {
                    System.Diagnostics.Debug.WriteLine("Hit the email mgr bool and SamName is " + SamName);
                    string UsrMgrDN = GetUsersManager(SamName);
                    System.Diagnostics.Debug.WriteLine("UsrMgrDN is: " + UsrMgrDN);
                    string MgrVal = GetManagersData(UsrMgrDN);//put this in the to mail address in production
                    MailAddress from = new MailAddress("no-reply@oai.aero");//these will be changed later to current users email
                    MailAddress to = new MailAddress("dengle@oai.aero");//needs to be changed later to MgrVal
                    MailMessage message = new MailMessage(from, to);//instantiates new email msg from System.net.mail
                    message.Subject = "Business purpose shipping request notice";//will want to change this to something else later
                    message.IsBodyHtml = true;//Saying the body will contain html
                    message.Body = @"<html>
                                    <body style='width:100%'>
                                        <h4 style='text-align:center'>Shipping Request</h4>
                                        <table style='width:100%'>
                                            <tr>
                                                <th style='text-align:center'>Sender:</th>
                                                <th style='text-align:center'>DESTINATION:</th>
                                                <th style='text-align:center'>REQUEST #/DELIVER BY:</th>
                                            </tr>
                                            <tr>
                                                <td style='text-align:center'>" + txtFromName.Text + "<br/>Omni Air Intl.<br/>Tulsa, OK</td>" +
                                                    "<td style='text-align:center'>" + txttoCompanyOrName.Text + "<br/>" + txtToAddressOne.Text + "<br/>" + txtCity.Text + ", " + ddlCountries.SelectedItem.Text + "</td>" +
                                                    "<td style='text-align:center'>" + txtShipID.Text + "<br/>" + calShipByDate.SelectedDate + "</td>" +
                                                "</tr>" +
                                            "</table>" +
                                        "</body>" +
                                    "</html>";
                    SmtpClient client = new SmtpClient();
                    client.Host = "smtprelay.oai.aero";
                    client.Port = 25;
                    System.Diagnostics.Debug.WriteLine("Sending an email message to " + to + " by using SMTP " + client.Host);
                    try
                    {
                        client.Send(message);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception caught in CreateTestMessage " + ex.ToString());
                    }
                }
                Server.Transfer("Completed.aspx");
            }
            else//if IsValid != true give this single error msg that is in the original location a the top of the page...may want to put it elsewhere on page
            {
                lblValidationSummary.Text = "Invalid items are outlined in red";
            }
        }
        // **********************************************   END Submit Process   *********************************************************
        // **********************************************   START ddlNumberItemsShipped_SelectedIndexChanged   *********************************************************
        //if more boxes are needed add more cases to the switch and ddl element on front-end, I believe 6 is adequate for purposes of internal uses. The result for multiple ship items is reflected in the way UPS Worldship import map 
        //looked at the relationship between the ship_request table for shipping info, and ship_Id in the ship items table as the FK of the 1-to-many relationship. UPS will expect to generate more than one shipping label for each 
        //package with the same shippng info
        protected void ddlNumberItemsShipped_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("hit the ddlNumberItemsShipped_SelectedIndexChanged method");
            if (ddlNumberItemsShipped.BorderColor == System.Drawing.Color.Red)
            {
                ddlNumberItemsShipped.BorderColor = System.Drawing.Color.Empty;
            }
            switch (ddlNumberItemsShipped.SelectedIndex)
            {
                case 1:
                    //txtDescriptionOne.Focus();
                    txtDescriptionOne.Visible = true;
                    txtQuantityOne.Visible = true;
                    //Hide rows             
                    txtDescriptionTwo.Visible = false;
                    txtQuantityTwo.Visible = false;
                    txtDescriptionThree.Visible = false;
                    txtQuantityThree.Visible = false;
                    txtDescriptionFour.Visible = false;
                    txtQuantityFour.Visible = false;
                    txtDescriptionFive.Visible = false;
                    txtQuantityFive.Visible = false;
                    txtDescriptionSix.Visible = false;
                    txtQuantitySix.Visible = false;
                    //Remove any text that might have been entered
                    txtDescriptionTwo.Text = string.Empty;
                    txtQuantityTwo.Text = string.Empty;
                    txtDescriptionThree.Text = string.Empty;
                    txtQuantityThree.Text = string.Empty;
                    txtDescriptionFour.Text = string.Empty;
                    txtQuantityFour.Text = string.Empty;
                    txtDescriptionFive.Text = string.Empty;
                    txtQuantityFive.Text = string.Empty;
                    txtDescriptionSix.Text = string.Empty;
                    txtQuantitySix.Text = string.Empty;
                    break;
                case 2:
                    //txtDescriptionOne.Focus();
                    txtDescriptionOne.Visible = true;
                    txtQuantityOne.Visible = true;
                    txtDescriptionTwo.Visible = true;
                    txtQuantityTwo.Visible = true;
                    txtDescriptionThree.Visible = false;
                    txtQuantityThree.Visible = false;
                    txtDescriptionFour.Visible = false;
                    txtQuantityFour.Visible = false;
                    txtDescriptionFive.Visible = false;
                    txtQuantityFive.Visible = false;
                    txtDescriptionSix.Visible = false;
                    txtQuantitySix.Visible = false;
                    //Remove any text that might have been input
                    txtDescriptionThree.Text = string.Empty;
                    txtQuantityThree.Text = string.Empty;
                    txtDescriptionFour.Text = string.Empty;
                    txtQuantityFour.Text = string.Empty;
                    txtDescriptionFive.Text = string.Empty;
                    txtQuantityFive.Text = string.Empty;
                    txtDescriptionSix.Text = string.Empty;
                    txtQuantitySix.Text = string.Empty;
                    break;
                case 3:
                    //txtDescriptionOne.Focus();
                    txtDescriptionOne.Visible = true;
                    txtQuantityOne.Visible = true;
                    txtDescriptionTwo.Visible = true;
                    txtQuantityTwo.Visible = true;
                    txtDescriptionThree.Visible = true;
                    txtQuantityThree.Visible = true;
                    txtDescriptionFour.Visible = false;
                    txtQuantityFour.Visible = false;
                    txtDescriptionFive.Visible = false;
                    txtQuantityFive.Visible = false;
                    txtDescriptionSix.Visible = false;
                    txtQuantitySix.Visible = false;
                    //Remove unecessary text
                    txtDescriptionFour.Text = string.Empty;
                    txtQuantityFour.Text = string.Empty;
                    txtDescriptionFive.Text = string.Empty;
                    txtQuantityFive.Text = string.Empty;
                    txtDescriptionSix.Text = string.Empty;
                    txtQuantitySix.Text = string.Empty;
                    break;
                case 4:
                    //txtDescriptionOne.Focus();
                    txtDescriptionOne.Visible = true;
                    txtQuantityOne.Visible = true;
                    txtDescriptionTwo.Visible = true;
                    txtQuantityTwo.Visible = true;
                    txtDescriptionThree.Visible = true;
                    txtQuantityThree.Visible = true;
                    txtDescriptionFour.Visible = true;
                    txtQuantityFour.Visible = true;
                    txtDescriptionFive.Visible = false;
                    txtQuantityFive.Visible = false;
                    txtDescriptionSix.Visible = false;
                    txtQuantitySix.Visible = false;
                    //Remove unecessary text
                    txtDescriptionFive.Text = string.Empty;
                    txtQuantityFive.Text = string.Empty;
                    txtDescriptionSix.Text = string.Empty;
                    txtQuantitySix.Text = string.Empty;
                    break;
                case 5:
                    //txtDescriptionOne.Focus();
                    txtDescriptionOne.Visible = true;
                    txtQuantityOne.Visible = true;
                    txtDescriptionTwo.Visible = true;
                    txtQuantityTwo.Visible = true;
                    txtDescriptionThree.Visible = true;
                    txtQuantityThree.Visible = true;
                    txtDescriptionFour.Visible = true;
                    txtQuantityFour.Visible = true;
                    txtDescriptionFive.Visible = true;
                    txtQuantityFive.Visible = true;
                    txtDescriptionSix.Visible = false;
                    txtQuantitySix.Visible = false;
                    //Remove unecessary text
                    txtDescriptionSix.Text = string.Empty;
                    txtQuantitySix.Text = string.Empty;
                    break;
                case 6:
                    //txtDescriptionOne.Focus();
                    txtDescriptionOne.Visible = true;
                    txtQuantityOne.Visible = true;
                    txtDescriptionTwo.Visible = true;
                    txtQuantityTwo.Visible = true;
                    txtDescriptionThree.Visible = true;
                    txtQuantityThree.Visible = true;
                    txtDescriptionFour.Visible = true;
                    txtQuantityFour.Visible = true;
                    txtDescriptionFive.Visible = true;
                    txtQuantityFive.Visible = true;
                    txtDescriptionSix.Visible = true;
                    txtQuantitySix.Visible = true;
                    break;
                default:
                    break;
            }
            //txtDescriptionOne.Focus();//this causes flicker to be present on postback but helps user to know the next step in form completion process. Will add back if needed
        }

        /// <summary>
        /// ddlUrgency_SelectedIndexChanged event handler added by Dan 10/29/2018 to add the selection to the comments for the warehouse
        /// </summary>
        protected void ddlUrgency_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlUrgency.BorderColor == System.Drawing.Color.Red)
            {
                ddlUrgency.BorderColor = System.Drawing.Color.Empty;
            }
        }
        // **********************************************   START ddlNumberItemsShipped_SelectedIndexChanged   *********************************************************
        #endregion

        #region " Methods "


        // **********************************************   START GetDepartments()   *********************************************************
        private void GetDepartments()//Modified byt Dan Engle to handle new dept ddl and limit the use of the db connections to when the method us used. Old sql connection were left open causing connection errors 
        {
            DataTable shippingDept = new DataTable();

            //SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["OAI_ShippingRequest"].ConnectionString);//Removed by Dan Engle 10/16/2018
            using(myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString))//Added by Dan to replace above connection
            {
                try
                {
                    //SqlDataAdapter adapter = new SqlDataAdapter("SELECT Department_ID, Department_Name FROM Shipping_Department WHERE Department_Type = 'P'", myConnection);//replaced by Dan engle 11/28/2018 to reflect db update of list
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT Department_ID, Department_Name FROM Shipping_Department WHERE Department_Type = 'U' ORDER BY Department_ID DESC", myConnection);
                    adapter.Fill(shippingDept);
                    ddlInitiatingDepartment.DataSource = shippingDept;
                    ddlInitiatingDepartment.DataTextField = "Department_Name";
                    ddlInitiatingDepartment.DataValueField = "Department_ID";
                    ddlInitiatingDepartment.DataBind();
                }
                catch (Exception ex)
                {
                    lblValidationSummary.Text = "Initiating Department drop down list was not populated - " + ex.Message;
                }
                myConnection.Dispose();
            }
        }
        private void GetISO3166_Countries()//added by Dan Engle because previously countries were hard coded to front end and abbreviations were incorrect to ISO-3166 standard, names were out-dated like Zaire, and updated required
                                           //developer to modify front in ddl of more than 200+ items. Now updating countries can be done from mgmt studio to dynamically populate ddl as needed
        {
            //System.Diagnostics.Debug.WriteLine("Successfully called GetISO3166_Countries");
            SqlConnection ISO3166_List = new SqlConnection(connectionSTR);//connectionSTR param defined globally now
            ShippingRequestDataContext db = new ShippingRequestDataContext(ISO3166_List);
            var All_ISO3166_Countries = from x in db.ISO3166_Countries
                                            //where >>>This may be filtered for nations UPS actually ships to...need investigte this
                                        select new
                                        {
                                            Code = x.IATA_Code,
                                            Name = x.ISO3166_Name
                                        };
            foreach (var y in All_ISO3166_Countries)
            {
                //System.Diagnostics.Debug.WriteLine("Found another country called " + y.Name);//this was used to verify country list pulled correctly from db
                ddlCountries.Items.Add(new ListItem(y.Name, y.Code));
            }
        }
        // **********************************************   END GetDepartments()   *********************************************************

        // **********************************************   START GetUserInformation()   *********************************************************
        private void GetUserInformation()//Modified byt Dan Engle to handle new dept ddl and limit the use of the db connections to when the method us used. Old sql connection were left open causing connection errors 
        {
            //SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["OAI_ShippingRequest"].ConnectionString);//Removed by Dan Engle 10/16/2018
            using (myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString))//Added by Dan to replace above connection
                try
                {
                    myConnection.Open();

                    SqlCommand cmd = new SqlCommand();

                    //CCALK 6/15/2012 If a record exists for this user - pull from DB and populate name, phone and email fields
                    cmd = new SqlCommand("uspSelectUserInformation", myConnection);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@User_ID", SqlDbType.NVarChar).Value = txtLoggedInUser.Text;

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            txtFromName.Text = reader["Name"].ToString();
                            txtFromPhone.Text = reader["Phone"].ToString();
                            txtEmailAddress.Text = reader["Email"].ToString();
                        }
                    }

                    reader.Close();

                    myConnection.Close();
                }

                catch (Exception ex)
                {
                    lblValidationSummary.Text = ex.Message;
                }
        }
        // **********************************************   END GetUserInformation()   *********************************************************

        // **********************************************   START SetInitialDDLRow()   *********************************************************
        private void SetInitialDDLRow()//modifed by Dan Engle to handle updates to ddls as needed
        {
            ddlInitiatingDepartment.Items.Insert(0, new ListItem("<Select initiating department>", "0"));
            ddlInitiatingDepartment.Focus();

            ddlCountries.Items.Insert(0, new ListItem("<Select Country or Territory>", "0"));//added by Dan to set default
            ddlCountries.SelectedValue = "0";

            ddlStates.Items.Insert(0, new ListItem("<Field may not be required>", "0"));
            ddlStates.SelectedValue = "0";

            ddlNumberItemsShipped.Items.Insert(0, new ListItem("<Number of boxes to ship>", "0"));

            ddlShipByTime.Items.Insert(0, new ListItem("<ETA of shipment>", "0"));

            ddlUrgency.Items.Insert(0, new ListItem("<Flexibility of arrival>", "0"));//added by Dan to set default
        }
        // **********************************************   END SetInitialDDLRow()   *********************************************************

        // **********************************************   START ValidateShippingAdresses()   *********************************************************
        //CCALK Validate shipping address fields before saving or updating to DB
        private void ValidateShippingAdresses()//Modified by Dan Engle to comply with shipping address requirements of UPS, address saved in db prior to modifying may not meet UPS requirements
        {
            //CCALK 6/20/2012 Output all data entry errors for the shipping address fields
            StringBuilder OAI_ValidationSummary = new StringBuilder("", 25);

            lblValidationSummaryShipping.Text = string.Empty;

            if (txttoCompanyOrName.Text == string.Empty)
            {
                txttoCompanyOrName.BorderColor = System.Drawing.Color.Red;
            }
            if (ddlCountries.SelectedValue == "0")
            {
                ddlCountries.BorderColor = System.Drawing.Color.Red;
            }
            if (txtToAddressOne.Text == string.Empty)
            {
                txtToAddressOne.BorderColor = System.Drawing.Color.Red;
            }
            if (txtCity.Text == string.Empty)
            {
                txtCity.BorderColor = System.Drawing.Color.Red;
            }
            if (ddlCountries.SelectedValue == "US" || ddlCountries.SelectedValue == "CA")//State is req for US and Canada
            {
                if (ddlStates.SelectedValue == "0")
                {
                    ddlStates.BorderColor = System.Drawing.Color.Red;
                }
                if (ddlCountries.SelectedValue == "US")
                {
                    if (NumberIsNumeric(txtPostalCode.Text) == false)//Only do a NumberIsNumeric check if shipped in US
                    {
                        txtPostalCode.Text = "US zipcodes must be numeric";
                        txtPostalCode.BorderColor = System.Drawing.Color.Red;
                    }
                }
            }
            if (txtPostalCode.Text == string.Empty)//UPS always requires zipcode
            {
                txtPostalCode.BorderColor = System.Drawing.Color.Red;
            }
            if (txtToPhone.Text == string.Empty)
            {
                txtToPhone.BorderColor = System.Drawing.Color.Red;
            }
        }

        private void ValidateShippingItemPair(TextBox Item, TextBox Qty)
        {
            if (Item.Text == string.Empty)
            {
                Item.BorderColor = System.Drawing.Color.Red;
                ValidPage = false;
            }
            if(NumberIsNumeric(Qty.Text) == false)
            {
                Qty.BorderColor = System.Drawing.Color.Red;
                ValidPage = false;
            }
            else
            {
                if(Item.Text != string.Empty)
                {
                    Item.BorderColor = System.Drawing.Color.Empty;
                }
                if (NumberIsNumeric(Qty.Text) == true)
                {
                    Qty.BorderColor = System.Drawing.Color.Empty;
                }                    
            }
        }
        // **********************************************   END ValidateShippingAdresses()   *********************************************************

        // **********************************************   START ValidatePage()   *********************************************************
        //CCALK Validate all fields before saving to DB - any errors sent to StringBuilder and sent to Validation Summary label
        private void ValidatePage()
        {
            //CCALK 6/7/2012 Output all data entry errors
            StringBuilder OAI_ValidationSummary = new StringBuilder("Invalid items outlines in red");

            lblValidationSummary.Text = string.Empty;

            if (ddlInitiatingDepartment.SelectedValue == "0")
            {
                ddlInitiatingDepartment.BorderColor = System.Drawing.Color.Red;
                DeptErrMsg.Visible = true;
                DeptErrMsg.Text = "Department is required";
                ValidPage = false;
            }
            if (ddlInitiatingDepartment.SelectedItem.Text == "Other")
            {
                string other = txtOtherInitiatingDepartment.Text;

                if (other.Length == 0)
                {
                    ddlInitiatingDepartment.BorderColor = System.Drawing.Color.Red;
                    DeptErrMsg.Visible = true;
                    DeptErrMsg.Text = "Must specify a department";
                    ValidPage = false;
                }
            }
            //if (ddlInitiatingDepartment.SelectedItem.Text == "Administration" || ddlInitiatingDepartment.SelectedItem.Text == "MX" || ddlInitiatingDepartment.SelectedItem.Text == "Operations")//commented out by Dan Engle 11/28/2018 subdept no longer needed
            //{
            //    if (ddlSubDepartments.SelectedValue == "0")
            //    {
            //        ddlInitiatingDepartment.BorderColor = System.Drawing.Color.Empty;
            //        ddlSubDepartments.BorderColor = System.Drawing.Color.Red;
            //        DeptErrMsg.Visible = true;
            //        DeptErrMsg.Text = "Subdepartment is required";
            //        ValidPage = false;
            //    }
            //}
            if (txtFromName.Text == string.Empty)
            {
                txtFromName.BorderColor = System.Drawing.Color.Red;
                txtFromName.Attributes.Add("placeholder", "Name is required");
                ValidPage = false;
            }
            if (txtFromPhone.Text == string.Empty)
            {
                txtFromPhone.BorderColor = System.Drawing.Color.Red;
                txtFromPhone.Attributes.Add("placeholder", "Phone is required");
                ValidPage = false;
            }
            if (txtEmailAddress.Text == string.Empty)
            {
                txtEmailAddress.BorderColor = System.Drawing.Color.Red;
                txtEmailAddress.Attributes.Add("placeholder", "Email is required");
                ValidPage = false;
            }
            if (txtToAttn.Text == string.Empty && ddlCountries.SelectedValue != "US")//This is a required field for intl shipments added by Dan Engle 11/16/2018
            {
                txtToAttn.BorderColor = System.Drawing.Color.Red;
                txtToAttn.Attributes.Add("placeholder", "required for business or international shipments");
                ValidPage = false;
            }
            if (ddlCountries.SelectedValue == "0")
            {
                ddlCountries.BorderColor = System.Drawing.Color.Red;
                ddlNationsErr.Visible = true;
                ddlNationsErr.Text = "Must select a country";
                ValidPage = false;
            }
            //CCALK 6/27/2012 Make sure either Business or Private is selected
            if (rblBusinessPrivate.SelectedIndex == -1)
            {
                PurposeErrLabel.Text = " Purpose is required";
                PurposeErrLabel.ForeColor = System.Drawing.Color.Red;
                ValidPage = false;
            }
            if (rblBusinessPrivate.SelectedValue == "B")
            {
                if (txtToAttn.Text == string.Empty)//This is the Company name in older form, can remove if not wanted later because UPS doesn't validate shipping purpose
                {
                    txtToAttn.BorderColor = System.Drawing.Color.Red;
                    txtToAttn.Attributes.Add("placeholder", "required for business or international shipments");
                    ValidPage = false;
                }
            }
            if (rblBusinessPrivate.SelectedValue == "P")
            {
                if (PayDedConfirm.Checked == false)
                {
                    PayDedErrMsg.Text = " Acknowledgement required";
                    PayDedErrMsg.ForeColor = System.Drawing.Color.Red;
                    ValidPage = false;
                }
            }
            if (txttoCompanyOrName.Text == string.Empty)
            {
                txttoCompanyOrName.BorderColor = System.Drawing.Color.Red;
                txttoCompanyOrName.Attributes.Add("placeholder", "Field is required");
                ValidPage = false;
            }
            if (txtToAddressOne.Text == string.Empty)
            {
                txtToAddressOne.BorderColor = System.Drawing.Color.Red;
                txtToAddressOne.Attributes.Add("placeholder", "Street address is required");
                ValidPage = false;
            }
            if (txtCity.Text == string.Empty)
            {
                txtCity.BorderColor = System.Drawing.Color.Red;
                txtCity.Attributes.Add("placeholder", "City is required");
                ValidPage = false;
            }
            if (ddlCountries.SelectedValue == "US" || ddlCountries.SelectedValue == "CA") //Canada and United States require province or state
            {
                if (ddlStates.SelectedValue == "0")
                {
                    ddlStates.BorderColor = System.Drawing.Color.Red;
                    ddlStatesErr.Visible = true;
                    ddlStatesErr.Text = "US and Canada require political subunits";
                    ValidPage = false;
                }
            }
            if (txtPostalCode.Text == string.Empty)//Postalcodes are required for all shipments added by Dan Engle 11/16/2018
            {
                txtPostalCode.BorderColor = System.Drawing.Color.Red;
                txtPostalCode.Attributes.Add("placeholder", "postal or zipcode required");
            }
            if (ddlCountries.SelectedValue == "US")
            {
                if (txtPostalCode.Text == string.Empty)
                {
                    txtPostalCode.BorderColor = System.Drawing.Color.Red;
                    txtPostalCode.Attributes.Add("placeholder", "postal or zipcode required");
                    ValidPage = false;
                }
                else if (NumberIsNumeric(txtPostalCode.Text) == false)//Only do a NumberIsNumeric check if shipped stateside
                {
                    txtPostalCode.BorderColor = System.Drawing.Color.Red;
                    txtPostalCode.Attributes.Add("placeholder", "US zipcodes must be numeric");
                    ValidPage = false;
                }
            }
            if (txtToPhone.Text == string.Empty)
            {
                txtToPhone.BorderColor = System.Drawing.Color.Red;
                txtToPhone.Attributes.Add("placeholder", "A phone number is required");
                ValidPage = false;
            }
            switch (ddlNumberItemsShipped.SelectedIndex)
            {
                case 0:
                    ddlNumberItemsShipped.BorderColor = System.Drawing.Color.Red;
                    ValidPage = false;
                    break;
                case 1:
                    ValidateShippingItemPair(txtDescriptionOne, txtQuantityOne);
                    break;
                case 2:
                    ValidateShippingItemPair(txtDescriptionOne, txtQuantityOne);
                    ValidateShippingItemPair(txtDescriptionTwo, txtQuantityTwo);
                    break;
                case 3:
                    ValidateShippingItemPair(txtDescriptionOne, txtQuantityOne);
                    ValidateShippingItemPair(txtDescriptionTwo, txtQuantityTwo);
                    ValidateShippingItemPair(txtDescriptionThree, txtQuantityThree);
                    break;
                case 4:
                    ValidateShippingItemPair(txtDescriptionOne, txtQuantityOne);
                    ValidateShippingItemPair(txtDescriptionTwo, txtQuantityTwo);
                    ValidateShippingItemPair(txtDescriptionThree, txtQuantityThree);
                    ValidateShippingItemPair(txtDescriptionFour, txtQuantityFour);
                    break;
                case 5:
                    ValidateShippingItemPair(txtDescriptionOne, txtQuantityOne);
                    ValidateShippingItemPair(txtDescriptionTwo, txtQuantityTwo);
                    ValidateShippingItemPair(txtDescriptionThree, txtQuantityThree);
                    ValidateShippingItemPair(txtDescriptionFour, txtQuantityFour);
                    ValidateShippingItemPair(txtDescriptionFive, txtQuantityFive);
                    break;
                case 6:
                    ValidateShippingItemPair(txtDescriptionOne, txtQuantityOne);
                    ValidateShippingItemPair(txtDescriptionTwo, txtQuantityTwo);
                    ValidateShippingItemPair(txtDescriptionThree, txtQuantityThree);
                    ValidateShippingItemPair(txtDescriptionFour, txtQuantityFour);
                    ValidateShippingItemPair(txtDescriptionFive, txtQuantityFive);
                    ValidateShippingItemPair(txtDescriptionSix, txtQuantitySix);
                    break;
            }
            //END Validate Items to ship controls
            if (calShipByDate.SelectedDate == Convert.ToDateTime("1/1/0001 12:00:00 AM"))
            {
                calShipByDate.BorderColor = System.Drawing.Color.Red;
                ValidPage = false;
            }
            if (ddlShipByTime.SelectedValue == "0")
            {
                ddlShipByTime.BorderColor = System.Drawing.Color.Red;
                ValidPage = false;
            }
            if (ddlUrgency.SelectedValue == "0")
            {
                ddlUrgency.BorderColor = System.Drawing.Color.Red;
                ValidPage = false;
            }
            //Only validate stateside - some countries do not have  numeric postal codes
            if (ddlCountries.SelectedValue == "US")
            {
                if (NumberIsNumeric(txtPostalCode.Text) == false)
                {
                    txtPostalCode.Text = "US zipcodes are numeric";
                    ValidPage = false;
                }
            }
            if (ddlCountries.SelectedValue == "US" && ValidDomesticPhoneNumber(txtFromPhone.Text) == false)
            {
                txtFromPhone.Text = "Please enter a valid phone domestic number";
                ValidPage = false;
            }
        }//modified by Dan to handle UPS requirments and changes made to front-end, back-end, and dB as needed. 
        // **********************************************   END ValidatePage()   *********************************************************

        //**********************************************   START ConvertTime()   *********************************************************
        private object ConvertTime(string p)//Left as-is by Dan Engle, could be improved if needed but times from ddl are hard-coded in front-end and 1hour accuracy seemed adequate. UPS requires 24-time and and using ddl prevents
                                            //user from putting in bad time for need to parse in validation method. If not broke, don't fix it!
        {
            //Converts standard time in ddlShipTime to military time and stores in this format 14:00:00
            string time = "";

            switch (p)
            {
                case "12:00 AM":

                    time = "12:00:00";

                    break;

                case "1:00 AM":

                    time = "1:00:00";

                    break;

                case "2:00 AM":

                    time = "2:00:00";

                    break;

                case "3:00 AM":

                    time = "3:00:00";

                    break;

                case "4:00 AM":

                    time = "4:00:00";

                    break;

                case "5:00 AM":

                    time = "5:00:00";

                    break;

                case "6:00 AM":

                    time = "6:00:00";

                    break;

                case "7:00 AM":

                    time = "7:00:00";

                    break;

                case "8:00 AM":

                    time = "8:00:00";

                    break;

                case "9:00 AM":

                    time = "9:00:00";

                    break;

                case "10:00 AM":

                    time = "10:00:00";

                    break;

                case "11:00 AM":

                    time = "11:00:00";

                    break;

                case "12:00 PM":

                    time = "12:00:00";

                    break;

                case "1:00 PM":

                    time = "13:00:00";

                    break;

                case "2:00 PM":

                    time = "14:00:00";

                    break;

                case "3:00 PM":

                    time = "15:00:00";

                    break;

                case "4:00 PM":

                    time = "16:00:00";

                    break;

                case "5:00 PM":

                    time = "17:00:00";

                    break;

                case "6:00 PM":

                    time = "18:00:00";

                    break;

                case "7:00 PM":

                    time = "19:00:00";

                    break;

                case "8:00 PM":

                    time = "20:00:00";

                    break;

                case "9:00 PM":

                    time = "21:00:00";

                    break;

                case "10:00 PM":

                    time = "22:00:00";

                    break;

                case "11:00 PM":

                    time = "23:00:00";

                    break;

            }
            return TimeSpan.Parse(time);
        }
        //**********************************************   END ConvertTime()   *********************************************************

        //**********************************************   START NumberIsNumeric()   *********************************************************
        bool NumberIsNumeric(string value)
        {
            bool isNumeric = false;

            Regex re = new Regex(@"^\d+$");
            isNumeric = re.Match(value).Success;

            return isNumeric;
        }
        //**********************************************   END NumberIsNumeric()   *********************************************************

        //**********************************************   START ValidDomesticPhoneNumber()   *********************************************************
        bool ValidDomesticPhoneNumber(string phoneNumber)
        {
            bool isValidPhoneNumber = false;

            Regex domestic = new Regex(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$");

            isValidPhoneNumber = domestic.IsMatch(phoneNumber);

            return isValidPhoneNumber;
        }
        //**********************************************   END ValidDomesticPhoneNumber()   *********************************************************
        #endregion

        #region " "To" Record Stuff "

        // **********************************************   START Save Shipping Addresses Process   *********************************************************
        protected void btnSave_Click(object sender, EventArgs e)
        {

            lblValidationSummaryShipping.Text = string.Empty;

            //CCALK Verify required fields have been populated with data
            ValidateShippingAdresses();

            if (lblValidationSummaryShipping.Text.Length > 0) { return; }
            //END

            //Save the current 'to' record to the DB
            //Shipping information is saved to the DB and tied to specific users by login name - ex: "ccalk"

            using (SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString))//modified by Dan Engle
            {

                myConnection.Open();

                SqlCommand cmd = new SqlCommand();

                cmd = new SqlCommand("uspInsertShippingClientAddress", myConnection);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@User_ID", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtLoggedInUser.Text);
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToAttn.Text);
                cmd.Parameters.Add("@Country", SqlDbType.NVarChar).Value = Server.HtmlDecode(ddlCountries.SelectedItem.Value);
                cmd.Parameters.Add("@Company", SqlDbType.NVarChar).Value = Server.HtmlDecode(txttoCompanyOrName.Text);
                cmd.Parameters.Add("@AddressOne", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToAddressOne.Text);
                cmd.Parameters.Add("@AddressTwo", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToAddressTwo.Text);
                cmd.Parameters.Add("@City", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtCity.Text);
                //CCALK 6/25/2012 If shipped internationally - State is not required - write empty string
                //if (ddlCountries.SelectedValue == "US")
                //{
                cmd.Parameters.Add("@State", SqlDbType.NVarChar).Value = Server.HtmlDecode(ddlStates.SelectedItem.Value);
                //}
                //else
                //{
                //    cmd.Parameters.Add("@State", SqlDbType.NVarChar).Value = Server.HtmlDecode("");
                //}
                //END
                cmd.Parameters.Add("@Postal_Code", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtPostalCode.Text);
                cmd.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToPhone.Text);

                cmd.ExecuteScalar();

                myConnection.Close();

                GetShippingAddresses();
            }

        }
        // **********************************************   END Save Shipping Addresses Process   *********************************************************

        // **********************************************   START Delete Shipping Address Process   *********************************************************
        //CCALK Delete selected shipping address from DB
        protected void btnDelete_Click(object sender, EventArgs e)
        {

            lblValidationSummaryShipping.Text = string.Empty;

            using (SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString))//added using Dan Engle
            {
                myConnection.Open();

                SqlCommand cmd = new SqlCommand();

                cmd = new SqlCommand("uspDeleteClientAdresses", myConnection);

                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    cmd.Parameters.Add("@Shipping_Client_Addresses_ID", SqlDbType.Int).Value = Convert.ToInt64(txtID.Text);

                    cmd.ExecuteNonQuery();
                    myConnection.Close();

                    //CCALK 66/19/2012 Clear address rows
                    txtToAttn.Text = string.Empty;
                    txttoCompanyOrName.Text = string.Empty;
                    txtToAddressOne.Text = string.Empty;
                    txtToAddressTwo.Text = string.Empty;
                    txtCity.Text = string.Empty;
                    //ddlCountries.SelectedItem.Text = "United States";//ddls will be set initial params Dan Engle 11/20/2018
                    //ddlStates.SelectedItem.Text = "Georgia";//ddls will be set initial params Dan Engle 11/20/2018
                    ddlCountries.Items.Clear();//added by Dan to clear any list items
                    ddlStates.Items.Clear();//added by Dan to clear any list items
                    ddlCountries.Items.Insert(0, new ListItem("<Select Country or Territory>", "0"));//added by Dan to set default
                    ddlCountries.SelectedValue = "0";
                    ddlStates.Items.Insert(0, new ListItem("<Field may not be required>", "0"));
                    ddlStates.SelectedValue = "0";
                    ddlNumberItemsShipped.Items.Insert(0, new ListItem("<Number of boxes to ship>", "0"));
                    ddlShipByTime.Items.Insert(0, new ListItem("<ETA of shipment>", "0"));
                    ddlUrgency.Items.Insert(0, new ListItem("<Flexibility of arrival>", "0"));//added by Dan to set default
                    txtPostalCode.Text = string.Empty;
                    txtToPhone.Text = string.Empty;

                    GetShippingAddresses();

                    ClearShippingFields();

                    myConnection.Close();

                }
                catch (FormatException)
                {
                    lblValidationSummaryShipping.Text = "You are trying to delete an address that does not exist in the database.";
                }
            }
        }
        // **********************************************   END Delete Shipping Address Process   *********************************************************

        // **********************************************   START UPDATE Shipping Address Process   *********************************************************
        //CCALK Update selected shipping address in DB
        protected void btnUpdate_Click(object sender, EventArgs e)
        {

            lblValidationSummaryShipping.Text = string.Empty;

            //CCALK Verify required fields have been populated with data
            ValidateShippingAdresses();

            if (lblValidationSummaryShipping.Text.Length > 0) { return; }
            //END

            //CCALK 6/18/2012 Update client address
            using (SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString))
            {
                myConnection.Open();//Connection closes 25 lines below

                SqlCommand cmd = new SqlCommand();

                cmd = new SqlCommand("uspUpdateClientAddress", myConnection);

                cmd.CommandType = CommandType.StoredProcedure;


                try
                {

                    cmd.Parameters.Add("@Shipping_Client_Addresses_ID", SqlDbType.Int).Value = Convert.ToInt16(txtID.Text);
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToAttn.Text);
                    cmd.Parameters.Add("@Country", SqlDbType.NVarChar).Value = Server.HtmlDecode(ddlCountries.SelectedItem.Value);
                    cmd.Parameters.Add("@Company", SqlDbType.NVarChar).Value = Server.HtmlDecode(txttoCompanyOrName.Text);
                    cmd.Parameters.Add("@AddressOne", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToAddressOne.Text);
                    cmd.Parameters.Add("@AddressTwo", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToAddressTwo.Text);
                    cmd.Parameters.Add("@City", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtCity.Text);
                    cmd.Parameters.Add("@State", SqlDbType.NVarChar).Value = Server.HtmlDecode(ddlStates.SelectedItem.Value);
                    cmd.Parameters.Add("@Postal_Code", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtPostalCode.Text);
                    cmd.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = Server.HtmlDecode(txtToPhone.Text);

                    cmd.ExecuteNonQuery();

                    myConnection.Close();//Verified connection closes from 25 lines above

                    GetShippingAddresses();

                }
                catch (FormatException)
                {
                    lblValidationSummaryShipping.Text = "You are trying to update an address that does not exist in the database. If this is a new address please click the Save button instead.";
                }
            }
        }
        // **********************************************   END UPDATE Shipping Address Process   *********************************************************

        // **********************************************   START Search Process   *********************************************************
        protected void btnSearch_Click(object sender, EventArgs e)
        {

            lblValidationSummaryShipping.Text = string.Empty;

            //Pulls all records in the database tied to the logged in user
            GetShippingAddresses();
        }

        private void GetShippingAddresses()
        {
            gvClientAddresses.Visible = true;

            SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString);

            myConnection.Open();

            SqlCommand myCommand = new SqlCommand("uspSelectClientAdresses", myConnection);

            myCommand.CommandType = CommandType.StoredProcedure;

            myCommand.Parameters.Add("@USER_ID", SqlDbType.NVarChar).Value = txtLoggedInUser.Text;

            DataSet myDataSet = new DataSet();

            SqlDataAdapter myAdapter = new SqlDataAdapter(myCommand);
            myAdapter.Fill(myDataSet);

            gvClientAddresses.DataSource = myDataSet;
            gvClientAddresses.DataBind();

            myConnection.Close();//Verified closed SQL connection 
        }
        // **********************************************   END Search Process   *********************************************************

        // **********************************************   START Row Click Process   ****************************************************
        //CCALK Display records based on selected Grid View row
        protected void OnRowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);
            GridViewRow gvRow = gvClientAddresses.Rows[index];
            txtID.Text = Server.HtmlDecode(gvRow.Cells[1].Text);
            txtToAttn.Text = Server.HtmlDecode(gvRow.Cells[2].Text);
            //System.Diagnostics.Debug.WriteLine(gvRow.Cells[3].Text + " " + gvRow.Cells[3].Text.Length);//used to check the string properties when getting data from gridview
            System.Diagnostics.Debug.WriteLine(gvRow.Cells[8].Text + " " + gvRow.Cells[8].Text.Length);//used to check the string properties when getting data from gridview
            ddlCountries.SelectedValue = Server.HtmlDecode(gvRow.Cells[3].Text.Trim());
            ddlStates.Items.Clear();
            SqlConnection Subunit_List = new SqlConnection(connectionSTR);
            ShippingRequestDataContext db = new ShippingRequestDataContext(Subunit_List);
            var This_Nations_Subunits = from x in db.Political_Subunits
                                        where x.IATA_Code == ddlCountries.SelectedValue
                                        select new
                                        {
                                            Nation = x.IATA_Code,
                                            Unit = x.Political_Unit_Code,
                                            Name = x.Political_Unit_Name
                                        };
            foreach (var y in This_Nations_Subunits)
            {
                if (y.Unit.Length > 1)
                {
                    ddlStates.Items.Add(new ListItem(y.Name, y.Unit.Substring(3)));
                    //System.Diagnostics.Debug.WriteLine("Hit > 0 length in foreach");
                }
                else
                {
                    ddlStates.Items.Insert(0, new ListItem("<Field may not be required>", "0"));
                    ddlStates.SelectedValue = "0";
                    //System.Diagnostics.Debug.WriteLine("Hit 0 length in foreach");
                }
            }
            if (gvRow.Cells[8].Text.Trim() == "&nbsp;")
            {
                ddlStates.Items.Insert(0, new ListItem("<Field may not be required>", "0"));
                ddlStates.SelectedValue = "0";
            }
            else if (gvRow.Cells[8].Text.Trim() != "&nbsp;")
            {
                ddlStates.SelectedValue = Server.HtmlDecode(gvRow.Cells[8].Text.Trim());
            }
            txttoCompanyOrName.Text = Server.HtmlDecode(gvRow.Cells[4].Text);
            txtToAddressOne.Text = Server.HtmlDecode(gvRow.Cells[5].Text);
            txtToAddressTwo.Text = Server.HtmlDecode(gvRow.Cells[6].Text);
            txtCity.Text = Server.HtmlDecode(gvRow.Cells[7].Text);
            txtPostalCode.Text = Server.HtmlDecode(gvRow.Cells[9].Text);
            txtToPhone.Text = Server.HtmlDecode(gvRow.Cells[10].Text);
            gvClientAddresses.Visible = false;
        }
        // **********************************************   END Row Click Process   *********************************************************


        protected void btnClearShippingFields_Click(object sender, EventArgs e)
        {
            ClearShippingFields();
        }

        private void ClearShippingFields()
        {

            lblValidationSummaryShipping.Text = string.Empty;

            txtToAttn.Text = string.Empty;
            ddlCountries.SelectedItem.Value = "0";
            txttoCompanyOrName.Text = string.Empty;
            txtToAddressOne.Text = string.Empty;
            txtToAddressTwo.Text = string.Empty;
            txtCity.Text = string.Empty;
            ddlStates.Items.Clear();
            ddlStates.Items.Insert(0, new ListItem("<Field may not be required>", "0"));
            ddlStates.SelectedValue = "0";
            txtPostalCode.Text = string.Empty;
            txtToPhone.Text = string.Empty;
        }


        #endregion
    }
}