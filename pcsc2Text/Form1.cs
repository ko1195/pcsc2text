using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Collections;

namespace pcsc2Text
{
    public partial class FrmMain : Form
    {

        FrmReport frmreport = new FrmReport();
        FrmConfiguration frmConfiguration = new FrmConfiguration(); 
        public FrmMain()
        {
            InitializeComponent();
        }

        private void ReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmreport.ShowDialog();
        }

        private void ConfigureToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmConfiguration.ShowDialog();
        }

        private void log(string ll)
        {
            textBox1.Text += ll;
        }
        private void logln(string ll)
        {
            log(ll + "\r\n");
        }

         
         
        Hashtable hashtable = null;

        void attendant()
        { 
            DataSet ds = (DataSet)dataGridView1.DataSource;
            DataTable tb = ds.Tables["journals25"];
            if (tb == null)
            {
                return;
            }


            hashtable = new Hashtable();
            Peaple p;

            foreach (DataRow row in tb.Rows)
            {
                string name = row["cardname"].ToString();
                string description = row["journalcode"].ToString();
                string icoFileName = row["cardno"].ToString();
                string installScript = row["logdatetime"].ToString();

                logln(name + ", " + description + ", " + icoFileName + ", " + installScript + ", ");

                int cardno = Int32.Parse(row["cardno"].ToString());

                if (hashtable[cardno] == null)
                { //ไม่มีข้อมูล
                    logln("เพิ่ม : " + cardno.ToString());
                    p = new Peaple(cardno, (DateTime)row["logdatetime"]); 
                    hashtable.Add(cardno, p);
                }
                else
                {
                    p = (Peaple)hashtable[cardno];
                    p.insert(row);
                    logln("update : " + cardno.ToString());
                }

            }
            logln("hash : " + hashtable.Count);
        }
        private void button1_Click(object sender, EventArgs e)
        {
             string connectionString = "Data Source=sp_nb\\veritrax;Initial Catalog=pcsc;Integrated Security=True";
            string sql = "SELECT * FROM journals where journalcode=25 and (CardName IS NOT NULL)";
            string sql2 = "SELECT * FROM journals where [LogDateTime] " + 
                "between CONVERT(datetime,'"+dtStart.Text + "') AND " +
                "CONVERT(datetime,'" + dtEnd.Text + "')";

            logln("Sql : "  + sql);
            logln("Sql2 : " + sql2);

            SqlConnection connection = new SqlConnection(connectionString);
            SqlDataAdapter dataadapter = new SqlDataAdapter(sql, connection);
            DataSet ds = new DataSet();

            connection.Open();
            dataadapter.Fill(ds, "journals25"); 
            connection.Close(); 

            logln("row count " + ds.Tables["journals25"].Rows.Count);

            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "journals25";

            
            
        }

        class Peaple
        { 
            private int cardno;
            private DateTime startDt;
            private DateTime endDt;
            private DataRow dr;
            private ArrayList rowlist;

            public Peaple(int cardno)
            {
                this.cardno = cardno;
                rowlist = new ArrayList();
            }

            public Peaple(int cardno, DateTime dt)
            {
                this.cardno = cardno;
                this.startDt = dt;
                this.endDt = dt;
                rowlist = new ArrayList();
            }
            public Peaple(int cardno, DataRow row)
            {
                this.cardno = cardno;
                rowlist = new ArrayList();
                rowlist.Add(row);
                startDt = (DateTime)row["logdatetime"];
                endDt = (DateTime)row["logdatetime"];
            }
            public void clear()
            {
                rowlist.Clear();
            }
            public void insert(DataRow row)
            {
                DateTime dt = (DateTime)row["logdatetime"];

                int a = DateTime.Compare(dt, startDt); 
                if(a<0 ) // กำหนดค่าให้ StartDate
                {
                    startDt = dt;
                    //logln("new enddate : " + dt.ToShortDateString());
                }
                else if(a>0) //// กำหนดค่าให้ EndDate
                {
                    endDt = dt;
                    //logln("new enddate : " + dt.ToShortDateString());
                }


                rowlist.Add(row); 
            }
            public ArrayList getAdtendantList()
            {
                return rowlist;
            }

            public int Cardno { get => cardno; set => cardno = value; }
            public DateTime StartDt { get => startDt; set => startDt = value; }
            public DateTime EndDt { get => endDt; set => endDt = value; }
            public DataRow Dr { get => dr; set => dr = value; }
            public ArrayList Rowlist { get => rowlist; set => rowlist = value; }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            int cnt =dataGridView1.ColumnCount;
            //dv = new DataView(ds.Tables[0], "journalcode = 25", "journalcode asc", DataViewRowState.CurrentRows);
            //dataGridView1.DataSource = dv;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //*** Thai Format
            System.Globalization.CultureInfo _cultureTHInfo = new System.Globalization.CultureInfo("th-TH");
            DateTime dateThai = Convert.ToDateTime(this.dtStart.Value, _cultureTHInfo);
            logln(dateThai.ToString("dd MMM yyyy", _cultureTHInfo));

            //*** Eng Format
            System.Globalization.CultureInfo _cultureEnInfo = new System.Globalization.CultureInfo("en-US");
            DateTime dateEng = Convert.ToDateTime(this.dtEnd.Value, _cultureEnInfo);
            logln(dateEng.ToString("dd MMM yyyy", _cultureEnInfo));


            dtStart.Format = DateTimePickerFormat.Custom;
            string[] formats = dtStart.Value.GetDateTimeFormats(Application.CurrentCulture);
            dtStart.CustomFormat = formats[0];

            DataTable dt = (DataTable)pcscDataSet1.Tables["tbAlJournals"];
            logln("cnt tblAttendant : " + dt.Rows.Count); 
        }

        private void button4_Click(object sender, EventArgs e)
        {
            attendant();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            String str ;
            logln("peaple cnt : " + hashtable.Count);
            foreach(Peaple p in hashtable.Values)
            {
                str = "card no : " + p.Cardno + ", start : " + p.StartDt.ToLongDateString() + ", end : "+ p.EndDt.ToLongDateString(); 
                logln(str);
                ArrayList ar = p.Rowlist;
                str = "";
                foreach(DataRow row in ar)
                {
                    DateTime dt = (DateTime)row["logDatetime"];
                    str += dt.ToShortDateString() + " " + dt.ToShortTimeString() + " , ";
                }
                logln(str); 
            }

        }
    }
}
