//
// N-tier C# and SQL program to analyze CTA Ridership Data
// 
// By: Antwan Love
// U. of Illinois, Chicago
// CS 341, Fall 2017
// Project # 08
//



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Data.SqlClient;

namespace CTA
{

  public partial class Form1 : Form
  {
    private string BuildConnectionString()
    {
      string version = "MSSQLLocalDB";
      string filename = this.txtDatabaseFilename.Text;

      string connectionInfo = String.Format(@"Data Source=(LocalDB)\{0};AttachDbFilename={1};Integrated Security=True;", version, filename);

      return connectionInfo;
    }

    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      //
      // setup GUI:
      //
      this.lstStations.Items.Add("");
      this.lstStations.Items.Add("[ Use File>>Load to display L stations... ]");
      this.lstStations.Items.Add("");

      this.lstStations.ClearSelected();

      toolStripStatusLabel1.Text = string.Format("Number of stations:  0");

      // 
      // open-close connect to get SQL Server started:
      //

      try
      {
        string filename = this.txtDatabaseFilename.Text;

        BusinessTier.Business bizTier;
        bizTier = new BusinessTier.Business(filename);

        bizTier.TestConnection();
      }
      catch
      {
        //
        // ignore any exception that occurs, goal is just to startup
        //
      }
    }


    //
    // File>>Exit:
    //
    private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
    {
      this.Close();
    }


    //
    // File>>Load Stations:
    //
    private void toolStripMenuItem2_Click(object sender, EventArgs e)
    {
      //
      // clear the UI of any current results:
      //
      ClearStationUI(true /*clear stations*/);

      //
      // now load the stations from the database:
      //
      try
      {

        BusinessTier.Business bizTier;
        bizTier = new BusinessTier.Business(this.txtDatabaseFilename.Text);

        var stations = bizTier.GetStations();
        string stationName;

        foreach (var station in stations) {
          stationName = (station.Name).Replace("'", "''");
          this.lstStations.Items.Add(stationName);
        }
       
        toolStripStatusLabel1.Text = string.Format("Number of stations:  {0:#,##0}", stations.Count);
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'.", ex.Message);
        MessageBox.Show(msg);
      }
    }


    //
    // User has clicked on a station for more info:
    //
    private void lstStations_SelectedIndexChanged(object sender, EventArgs e)
    {
      // sometimes this event fires, but nothing is selected...
      if (this.lstStations.SelectedIndex < 0)   // so return now in this case:
        return; 
      
      //
      // clear GUI in case this fails:
      //
      ClearStationUI();

      //
      // now display info about selected station:
      //
      string stationName = this.lstStations.Text;
      stationName = stationName.Replace("'", "''");

      try
      {
        // ----- TODO -----
        BusinessTier.Business bizTier = new BusinessTier.Business(this.txtDatabaseFilename.Text);
        var TotalRiderships = bizTier.GetTotalRiderships();

        // 
        // now we need total and avg for this station:
        //
        // ----- TODO -----
        var RiderData = bizTier.GetRidershipData(stationName);
        var stationTotal = RiderData.ElementAt(0).TotalRidership;
        double stationAvg = RiderData.ElementAt(0).AvgRidership;
        double percentage = ((double)stationTotal) / TotalRiderships * 100.0;

        this.txtTotalRidership.Text = stationTotal.ToString("#,##0");
        this.txtAvgDailyRidership.Text = string.Format("{0:#,##0}/day", stationAvg);
        this.txtPercentRidership.Text = string.Format("{0:0.00}%", percentage);

        //
        // now ridership values for Weekday, Saturday, and
        // sunday/holiday:
        //
        // ----- TODO -----

        var weekdays = bizTier.GetWeekday(stationName);

        var holidays = bizTier.GetHoliday(stationName);

        var saturdays = bizTier.GetSaturday(stationName);

        var stations = bizTier.GetStations();
        var found = stations.Single(station => station.Name == this.lstStations.Text);
        var stops = bizTier.GetStops(found.ID);
        
        this.txtStationID.Text = (found.ID).ToString();
        this.txtSaturdayRidership.Text = saturdays.ToString("#,##0");
        this.txtSundayHolidayRidership.Text = holidays.ToString("#,##0");
        this.txtWeekdayRidership.Text = weekdays.ToString("#,##0");

        //
        // finally, what stops do we have at this station?
        //
        foreach (var f in stops)
        {
          this.lstStops.Items.Add(f.Name);
        }
        
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'.", ex.Message);
        MessageBox.Show(msg);
      }
    }

    private void ClearStationUI(bool clearStatations = false)
    {
      ClearStopUI();

      this.txtTotalRidership.Clear();
      this.txtTotalRidership.Refresh();

      this.txtAvgDailyRidership.Clear();
      this.txtAvgDailyRidership.Refresh();

      this.txtPercentRidership.Clear();
      this.txtPercentRidership.Refresh();

      this.txtStationID.Clear();
      this.txtStationID.Refresh();

      this.txtWeekdayRidership.Clear();
      this.txtWeekdayRidership.Refresh();
      this.txtSaturdayRidership.Clear();
      this.txtSaturdayRidership.Refresh();
      this.txtSundayHolidayRidership.Clear();
      this.txtSundayHolidayRidership.Refresh();

      this.lstStops.Items.Clear();
      this.lstStops.Refresh();

      if (clearStatations)
      {
        this.lstStations.Items.Clear();
        this.lstStations.Refresh();
      }
    }


    //
    // user has clicked on a stop for more info:
    //
    private void lstStops_SelectedIndexChanged(object sender, EventArgs e)
    {
      // sometimes this event fires, but nothing is selected...
      if (this.lstStops.SelectedIndex < 0)   // so return now in this case:
        return; 

      //
      // clear GUI in case this fails:
      //
      ClearStopUI();

      //
      // now display info about this stop:
      //
      string stopName = this.lstStops.Text;
      stopName = stopName.Replace("'", "''");

      try
      {
        BusinessTier.Business bizTier = new BusinessTier.Business(this.txtDatabaseFilename.Text);
        var stations = bizTier.GetStations();
        var found = stations.Single(station => station.Name == this.lstStations.Text);
        var stops = bizTier.GetStops(found.ID);
        var selectedStop = stops.Single(s => s.Name == this.lstStops.Text);

        if (selectedStop.ADA)
          this.txtAccessible.Text = "Yes";
        else
          this.txtAccessible.Text = "No";

        // direction of travel:
        this.txtDirection.Text = selectedStop.Direction;

        // lat/long position:
        this.txtLocation.Text = string.Format("({0:00.0000}, {1:00.0000})",
          selectedStop.Latitude, selectedStop.Longitude);

        //
        // now we need to know what lines are associated 
        // with this stop:
        int stopID = selectedStop.ID;

        // ----- TODO -----

        var shades = bizTier.GetColors(stopID);

        foreach (var s in shades) {
          this.lstLines.Items.Add(s.Color);
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'.", ex.Message);
        MessageBox.Show(msg);
      }
      //finally
      //{
      //  if (db != null && db.State == ConnectionState.Open)
      //    db.Close();
      //}
    }

    private void ClearStopUI()
    {
      this.txtAccessible.Clear();
      this.txtAccessible.Refresh();

      this.txtDirection.Clear();
      this.txtDirection.Refresh();

      this.txtLocation.Clear();
      this.txtLocation.Refresh();

      this.lstLines.Items.Clear();
      this.lstLines.Refresh();
    }


    //
    // Top-10 stations in terms of ridership:
    //
    private void top10StationsByRidershipToolStripMenuItem_Click(object sender, EventArgs e)
    {
      //
      // clear the UI of any current results:
      //
      ClearStationUI(true /*clear stations*/);

      //
      // now load top-10 stations:
      //

      try
      {
        BusinessTier.Business bizTier;
        bizTier = new BusinessTier.Business(this.txtDatabaseFilename.Text);

        var topStations = bizTier.GetTopStations(10);

        foreach (var station in topStations) {
          this.lstStations.Items.Add(station.Name);
        }

        toolStripStatusLabel1.Text = string.Format("Number of stations:  {0:#,##0}", topStations.Count);
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'.", ex.Message);
        MessageBox.Show(msg);
      }
      
    }

    private void txtTotalRidership_TextChanged(object sender, EventArgs e)
    {
         
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      string key = this.textBox1.Text;

      BusinessTier.Business bizTier;
      bizTier = new BusinessTier.Business(this.txtDatabaseFilename.Text);

      var newStations = bizTier.SearchStations(key);

      ClearStationUI(true /*clear stations*/);

      foreach (var n in newStations) {
        lstStations.Items.Add(n.Name);
      }


    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {
      BusinessTier.Business bizTier = new BusinessTier.Business(this.txtDatabaseFilename.Text);
      var stations = bizTier.GetStations();
      var found = stations.Single(station => station.Name == this.lstStations.Text);
      var stops = bizTier.GetStops(found.ID);
      var selectedStop = stops.Single(s => s.Name == this.lstStops.Text);

      string change = this.textBox2.Text;
      int update = 0;

      if ((change == "yes") || (change == "no") || (change == "Yes") || (change == "No")) {
        if ((change == "yes") || (change == "Yes")) {
          update = 1;
          var AccessUpdate = bizTier.updateSuccess(change, update);

          if (selectedStop.ADA) {
            this.txtAccessible.Text = change;
            // uncomment below, comment above to confirm update successful , 
            // this.txtAccessible.Text = "*YesUP";

          }

        } else if ((change == "No") || (change == "no"))  {
          update = 0;
          var AccessBad = bizTier.updateSuccess(change, update);

          if (selectedStop.ADA)
          {
            this.txtAccessible.Text = change;
            // uncomment below, comment above to confirm update successful , 
            // this.txtAccessible.Text = "NoDown*";
          }

        }
      }
    }

    private void txtAccessible_TextChanged(object sender, EventArgs e)
    {

    }
  }//class
}//namespace
