using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using ImprovedShipper.Models;

namespace ImprovedShipper
{
    public partial class Retrieve : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            dbquery(System.DateTime.Now.AddDays(-45), System.DateTime.Now);
        }
        public void dbquery(DateTime fromDate, DateTime toDate)
        {
            string connectionSTR = ConfigurationManager.ConnectionStrings["ImprovedShipper.Properties.Settings.OAI_ShippingRequestConnectionString"].ConnectionString;

            SqlConnection allrequests = new SqlConnection(connectionSTR);
            ShippingRequestDataContext db = new ShippingRequestDataContext(allrequests);

            var query = from x in db.Processed_Requests
                        where x.Ship_Type == "P"
                        where x.Ship_InitDate.Value > fromDate
                        where x.Ship_InitDate.Value <= toDate
                        where x.From_SamName != null //I know this is stupid, but UPS populates this data for 1st package on multiple package shipments and creates additional records with null fields in these columns --Dan Engle 11/29/2018
                        select new
                        {
                            PK = x.ID,
                            id = x.Original_ShipID,
                            Sender = x.From_SamName,
                            //Purpose = x.Ship_Type,//Presumably these are all personal requests for payroll purposes, no reason to display it
                            EmpID = x.From_EmpID,
                            Status = x.Payroll_Status,//Presumably these are all 'Waiting' to be processed, no reason to display it
                            InitDate = x.Ship_InitDate.Value
                        };

            Literal1.Text = "<table id='main-table' class='display table table-hover text-center'>";
            Literal1.Text += "<thead>";
            Literal1.Text += "<tr>";
            Literal1.Text += "<th>Request #: </th>";
            Literal1.Text += "<th>Sender: </th>";
            //Literal1.Text += "<th>Request Date</th>";//removed this column to hide sender name
            Literal1.Text += "<th>Employee ID: </th>";
            Literal1.Text += "<th>Status: </th>";
            Literal1.Text += "</tr>";
            Literal1.Text += "</thead>";
            Literal1.Text += "<tbody>";
            foreach (var rec in query)
            {
                Literal1.Text += "<tr>";
                Literal1.Text += "<td><a href='Update.aspx?field1=" + rec.id + "'>" + rec.id + "</a></td>";
                //Literal1.Text += "<td>" + rec.Sender + "</td>";//for possibly not want to display this
                Literal1.Text += "<td>" + rec.InitDate.ToShortDateString() + "</td>";
                Literal1.Text += "<td>" + rec.EmpID + "</td>";
                Literal1.Text += "<td>" + rec.Status + "</td>";
                Literal1.Text += "</tr>";
            }
            Literal1.Text += "</tbody>";
            Literal1.Text += "</table>";
            foreach (var q in query)
            {
                System.Diagnostics.Debug.WriteLine(">>>>> " + q.Sender + " <<<<<");
            }
            //GridView1.DataSource = querry;
            //GridView1.DataBind();

            db.Dispose();

        }
    }
}