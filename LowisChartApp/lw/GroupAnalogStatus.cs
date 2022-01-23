using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using LowisChartApp.model;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using DevExpress.XtraCharts;
using System.Collections;
using System.Configuration;

//test perubahan
namespace LowisChartApp.lw
{
    public partial class GroupAnalogStatus : DevExpress.XtraEditors.XtraUserControl
    {
        DB dbCon;
        public GroupAnalogStatus()
        {
            InitializeComponent();
            dbCon = new DB(Globals.getstringco());
        }

        public void HighlightGrid()
        {
            if (Globals.SelectListView != "")
            {
                int rowHandle = GetRowHandleByColumnValue(gvLowis, "FacilityWellName", Globals.SelectListView);
                gvLowis.FocusedRowHandle = rowHandle;
            }
        }
        private int GetRowHandleByColumnValue(GridView view, string ColumnFieldName, object value)
        {
            int result = GridControl.InvalidRowHandle;
            for (int i = 0; i < view.RowCount; i++)
            {
                //string tmp = view.GetDataRow(i)[ColumnFieldName].ToString();
                if (view.GetDataRow(i)[ColumnFieldName].ToString().Trim().Equals(value))
                    return i;
            }
            return result;
        }
        private void GroupAnalogStatus_Load(object sender, EventArgs e)
        {
            dbCon.clearCache();
            Dictionary<string, object> paras = new Dictionary<string, object>();
            if (Globals.formPilih == "gas")
            {
                paras.Add("@param", "");
            }
            else
            {
                paras.Add("@param", Globals.ForeignKey);
            }
            DataSet ds = dbCon.execprosedure("dbo.[prosedur_getdataGroupanalogstatus]", paras);
            if (ds.Tables.Count < 1)
            {
                //MessageBox.Show(ds.Tables[0].Rows.Count.ToString());
                MessageBox.Show("Database execute timeout", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            gdLowis.DataSource = ds.Tables[0];
            gdLowis.Cursor = Cursors.Hand;
            this.gdLowis.Refresh();
        }

        private void gdLowis_Click(object sender, EventArgs e)
        {
            foreach (int i in gvLowis.GetSelectedRows())
            {
                DataRow row = gvLowis.GetDataRow(i);
                Globals.ForeignKey = row[1].ToString().Trim();
            }
        }

        private void gdLowis_MouseUp(object sender, MouseEventArgs e)
        {
            foreach (int i in gvLowis.GetSelectedRows())
            {
                DataRow row = gvLowis.GetDataRow(i);
                Globals.ForeignKey = row[1].ToString().Trim();
            }
        }

        private void gvLowis_RowClick(object sender, RowClickEventArgs e)
        {
            Int32[] selectedRowHandles = gvLowis.GetSelectedRows();
            ArrayList rows = new ArrayList();
            string FacilityWellName = "", desc="";
           
            for (int i = 0; i < selectedRowHandles.Length; i++)
            {
                int selectedRowHandle = selectedRowHandles[i];
                if (selectedRowHandle >= 0)
                    rows.Add(gvLowis.GetDataRow(selectedRowHandle));
            }

            for (int i = 0; i < rows.Count; i++)
            {
                DataRow row = rows[i] as DataRow;
                FacilityWellName = row["FacilityWellName"].ToString();
                desc = row["AnalogPointDescription"].ToString();
            }

            //Create a line series.
            Series series1 = new Series("Series 1", ViewType.Spline);
            //Series series2 = new Series("Series 2", ViewType.Spline);


            dbCon.clearCache();
            Dictionary<string, object> paras = new Dictionary<string, object>();
            paras.Add("@wellname", FacilityWellName);
            paras.Add("@Analog_Desc", desc);
            DataSet ds = dbCon.execprosedure("dbo.[prosedur_getdataanalogtrendnew]", paras);

            if (ds.Tables.Count < 1)
            {
                //MessageBox.Show(ds.Tables[0].Rows.Count.ToString());
                MessageBox.Show("Database execute timeout for chart", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    //TextBox1.Text = row["ImagePath"].ToString();
                    series1.Points.Add(new SeriesPoint(row["timestamp"], row["VALUE"]));
                }
            }

            
            //// Add the series to the chart.
            chartAnalogStatus.Series.Add(series1);
            //lineChart.Series.Add(series2);

            //// Set the numerical argument scale types for the series,
            //// as it is qualitative, by default.
            series1.ArgumentScaleType = ScaleType.DateTime;

            //// Access the view-type-specific options of the series.
            ((LineSeriesView)series1.View).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
            ((LineSeriesView)series1.View).LineMarkerOptions.Kind = MarkerKind.Circle;
            ((LineSeriesView)series1.View).LineStyle.DashStyle = DashStyle.Solid;

            //// Access the type-specific options of the diagram.
            ((XYDiagram)chartAnalogStatus.Diagram).EnableAxisXZooming = true;

            //// Hide the legend (if necessary).
            chartAnalogStatus.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;

            //// Add a title to the chart (if necessary).
            chartAnalogStatus.Titles.Add(new ChartTitle());
            chartAnalogStatus.Titles[0].Text = FacilityWellName + "-" + desc;
        }
    }
}
