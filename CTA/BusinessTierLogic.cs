//
// BusinessTier:  business logic, acting as interface between UI and data store.
//

using System;
using System.Collections.Generic;
using System.Data;


namespace BusinessTier
{

  //
  // Business:
  //
  public class Business
  {
    //
    // Fields:
    //
    private string _DBFile;
    private DataAccessTier.Data dataTier;


    ///
    /// <summary>
    /// Constructs a new instance of the business tier.  The format
    /// of the filename should be either |DataDirectory|\filename.mdf,
    /// or a complete Windows pathname.
    /// </summary>
    /// <param name="DatabaseFilename">Name of database file</param>
    /// 
    public Business(string DatabaseFilename)
    {
      _DBFile = DatabaseFilename;

      dataTier = new DataAccessTier.Data(_DBFile);
    }


    ///
    /// <summary>
    ///  Opens and closes a connection to the database, e.g. to
    ///  startup the server and make sure all is well.
    /// </summary>
    /// <returns>true if successful, false if not</returns>
    /// 
    public bool TestConnection()
    {
      return dataTier.OpenCloseConnection();
    }


    ///
    /// <summary>
    /// Returns all the CTA Stations, ordered by name.
    /// </summary>
    /// <returns>Read-only list of CTAStation objects</returns>
    /// 
    public IReadOnlyList<CTAStation> GetStations()
    {
      List<CTAStation> stations = new List<CTAStation>();

      try
      {
        //
        // TODO!
        //

        string sql = "SELECT StationID, Name FROM Stations ORDER BY Name ASC";


        var data = dataTier.ExecuteNonScalarQuery(sql);
       

        foreach (DataRow row in data.Tables["TABLE"].Rows)
        {
          stations.Add(new CTAStation((Convert.ToInt32(row["StationID"].ToString())) ,  (Convert.ToString(row["Name"].ToString()))));
        }



      }
      catch (Exception ex)
      {
        string msg = string.Format("Error in Business.GetStations: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }

      return stations;
    }


    ///
    /// <summary>
    /// Returns the CTA Stops associated with a given station,
    /// ordered by name.
    /// </summary>
    /// <returns>Read-only list of CTAStop objects</returns>
    ///
    public IReadOnlyList<CTAStop> GetStops(int stationID)
    {
      List<CTAStop> stops = new List<CTAStop>();

      try
      {

        //
        // TODO!
        //
        string sqlName = string.Format(@"SELECT Name FROM Stations WHERE StationID = {0}", stationID);
        string sqlID = string.Format(@"SELECT StationID FROM Stations WHERE StationID = {0}", stationID);
        var foundName = dataTier.ExecuteScalarQuery(sqlName);
        var foundID = dataTier.ExecuteScalarQuery(sqlID);
        var tempN = Convert.ToString(foundName);
        tempN = tempN.Replace("'", "''");

        var tempID = Convert.ToInt32(foundID);

        string sql = string.Format(@"SELECT * 
                                    FROM Stops
                                    INNER JOIN Stations ON Stops.StationID = {0}
                                    WHERE Stations.Name = '{1}'
                                    ORDER BY Stops.Name ASC;", tempID, tempN);

        var stopsFinal = dataTier.ExecuteNonScalarQuery(sql);

        foreach (DataRow row in stopsFinal.Tables["TABLE"].Rows)
        {
          stops.Add(new CTAStop((Convert.ToInt32(row["StopID"].ToString())),
                                  (Convert.ToString(row["Name"].ToString())),
                                  (Convert.ToInt32(row["StationID"].ToString())),
                                  (Convert.ToString(row["Direction"].ToString())),
                                  (Convert.ToBoolean(row["ADA"].ToString())),
                                  (Convert.ToDouble(row["Latitude"].ToString())),
                                  (Convert.ToDouble(row["Longitude"].ToString())) ));
        }


      }
      catch (Exception ex)
      {
        string msg = string.Format("Error in Business.GetStops: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }

      return stops;
    }


    ///
    /// <summary>
    /// Returns the top N CTA Stations by ridership, 
    /// ordered by name.
    /// </summary>
    /// <returns>Read-only list of CTAStation objects</returns>
    /// 
    public IReadOnlyList<CTAStation> GetTopStations(int N)
    {
      if (N < 1)
        throw new ArgumentException("GetTopStations: N must be positive");

      List<CTAStation> stations = new List<CTAStation>();

      try
      {

        //
        // TODO!
        //

        string sql = string.Format(@"SELECT TOP {0} Name, Stations.StationID FROM STATIONS 
                                      Inner Join Riderships ON Riderships.StationID = Stations.StationID
                                        Group By Name, Stations.StationID
                                        ORDER BY (SUM(DailyTotal)) DESC;", N);

        var topStation = dataTier.ExecuteNonScalarQuery(sql);

        foreach (DataRow row in topStation.Tables["TABLE"].Rows)
        {
          stations.Add(new CTAStation((Convert.ToInt32(row["StationID"].ToString())), (Convert.ToString(row["Name"].ToString()))));
        }


      }
      catch (Exception ex)
      {
        string msg = string.Format("Error in Business.GetTopStations: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }

      return stations;
    }

    // Function to return total amount of Riderships
    public long GetTotalRiderships() {
      long riderTotal = 0;

      try {
        string sql = string.Format(@"SELECT Sum(Convert(bigint,DailyTotal)) As TotalOverall
                                    FROM Riderships;
                                    ");

        var temp = dataTier.ExecuteScalarQuery(sql);

        riderTotal = Convert.ToInt64(temp);

      }
      catch (Exception ex)
      {
        string msg = string.Format("Error in Business.GetTotalRiderships: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }


      return riderTotal;
    }

    // Function to return a list with the total ridership and average for selected station
    public IReadOnlyList<CTAData> GetRidershipData(string station) {
      List<CTAData> riderData = new List<CTAData>();

      try {
        string sql = string.Format(@"
                              SELECT Sum(DailyTotal) As TotalRiders, 
                                     Avg(DailyTotal) As AvgRiders
                              FROM Riderships
                              INNER JOIN Stations ON Riderships.StationID = Stations.StationID
                              WHERE Name = '{0}';
                              ", station);

        var result = dataTier.ExecuteNonScalarQuery(sql);
        

        foreach (DataRow row in result.Tables["TABLE"].Rows) {
          riderData.Add(new CTAData( ( Convert.ToInt32(row["TotalRiders"].ToString()) ) , ( Convert.ToInt32(row["AvgRiders"].ToString() ) ) ) );
        }


      }
      catch (Exception ex) {
        string msg = string.Format("Error in Business.GetRidershipData: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }
      return riderData;
    }

    // function to get colors
    public IReadOnlyList<CTAColors> GetColors(int stopID) {
      List<CTAColors> hue = new List<CTAColors>();

      string sql = string.Format(@"
                            SELECT Color
                            FROM Lines
                            INNER JOIN StopDetails ON Lines.LineID = StopDetails.LineID
                            INNER JOIN Stops ON StopDetails.StopID = Stops.StopID
                            WHERE Stops.StopID = {0}
                            ORDER BY Color ASC;
                            ", stopID);

      var shades = dataTier.ExecuteNonScalarQuery(sql);

      foreach (DataRow row in shades.Tables["TABLE"].Rows)
      {
        hue.Add(new CTAColors(row["Color"].ToString()));
      }


      return hue;
    }

    // Function to obtain the weekday riderships
    public int GetWeekday(string stationName) {
      // obtain weekday of ridership
      int weekdays = 0;

      string sqlW = string.Format(@"Select Sum(DailyTotal)
                             From Riderships 
                              Inner JOIN Stations  On Riderships.StationID = Stations.StationID AND
                               (Riderships.TypeOfDay = 'W' AND 
                                Stations.Name =  '{0}');", stationName);

      var numWeek = dataTier.ExecuteScalarQuery(sqlW);
      weekdays = Convert.ToInt32(numWeek);

      return weekdays;
    }

    // Function to obtain the saturday riderships
    public int GetSaturday(string stationName)
    {
      // obtain weekday of ridership
      int satday = 0;

      // obtain weekend of ridership
      string sqlS = string.Format(@"Select Sum(DailyTotal)
                             From Riderships 
                              Inner JOIN Stations  On Riderships.StationID = Stations.StationID AND
                               (Riderships.TypeOfDay = 'A' AND 
                                Stations.Name =   '{0}');", stationName);

      var numWeek = dataTier.ExecuteScalarQuery(sqlS);
      satday = Convert.ToInt32(numWeek);

      return satday;
    }

    // Function to obtain the holiday riderships
    public int GetHoliday(string stationName)
    {
      // obtain weekday of ridership
      int holiday = 0;

      // obtain holiday of ridership
      string sqlH = string.Format(@"Select Sum(DailyTotal)
                             From Riderships 
                              Inner JOIN Stations  On Riderships.StationID = Stations.StationID AND
                               (Riderships.TypeOfDay = 'U' AND 
                                Stations.Name =  '{0}');", stationName);

      var numWeek = dataTier.ExecuteScalarQuery(sqlH);
      holiday = Convert.ToInt32(numWeek);

      return holiday;
    }

    ///
    /// <summary>
    /// Returns all the CTA Stations, ordered by name from searchbox.
    /// </summary>
    /// <returns>Read-only list of CTAStation objects</returns>
    /// 
    public IReadOnlyList<CTAStation> SearchStations(string key)
    {
      List<CTAStation> stations = new List<CTAStation>();

      try
      {
        //
        // TODO!
        //

        string sql = string.Format(@"SELECT StationID, Name 
                                    FROM Stations WHere Name like '%{0}%' ORDER BY Name ASC;", key) ;


        var data = dataTier.ExecuteNonScalarQuery(sql);


        foreach (DataRow row in data.Tables["TABLE"].Rows)
        {
          stations.Add(new CTAStation((Convert.ToInt32(row["StationID"].ToString())), (Convert.ToString(row["Name"].ToString()))));
        }



      }
      catch (Exception ex)
      {
        string msg = string.Format("Error in Business.GetStations: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }

      return stations;
    }

    // update

    public bool updateSuccess(string key, int change) {
      //int update = 0;

      string sql = string.Format(@"UPDATE Stops SET Stops.ADA = {0} WHERE Stops.Name = '{1}';", change, key);

      var update = dataTier.ExecuteActionQuery(sql);

      if (update == 1) {
        return true;
      } else {
        return false;
      }

      //return false;
    }

  }//class
}//namespace
