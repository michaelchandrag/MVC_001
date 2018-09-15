using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Collections;

namespace MVC_001.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (ASPCRUDEntities db = new ASPCRUDEntities())
            {
                List<storage> listStorage = db.storages.ToList<storage>();
                ViewBag.listStorage = listStorage;
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        private bool isValidContentType(string contentType)
        {
            return contentType.Equals("png") || contentType.Equals("jpg") || contentType.Equals("jpeg");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public int? TryParseNullable(string val)
        {
            int outValue;
            return int.TryParse(val, out outValue) ? (int?)outValue : null;
        }
        
        [HttpPost]
        public ActionResult Edit(String storage_location)
        {
            try
            {
                using (ASPCRUDEntities db = new ASPCRUDEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var model = (from u in db.storages
                     where u.storage_location == storage_location
                     select u).First();
                    return Json(model);
                }
                /*var context = new ASPCRUDEntities();
                context.Configuration.ProxyCreationEnabled = false;
                storage model = context.storages.Where(x => x.storage_location == storage_location).FirstOrDefault();
                return Json(model);*/
            }
            catch (Exception ex)
            {
                return Json(new { Message = "Error"});
            }
        }

        [HttpPost]
        public ActionResult Update(String storage_location,String warehouse)
        {
            try
            {
                using (ASPCRUDEntities db = new ASPCRUDEntities())
                {
                    var result = db.storages.SingleOrDefault(b => b.storage_location == storage_location);
                    if (result != null)
                    {
                        result.warehouse = warehouse;
                        db.SaveChanges();
                    }
                }
                return Content("success");
            }
            catch (Exception ex)
            {
                return Content("error"); 
            }
        }

        [HttpPost]
        public ActionResult Delete(string storage_location)
        {
            try
            {
                using (ASPCRUDEntities db = new ASPCRUDEntities())
                {
                    var storage = new storage {storage_location = storage_location };
                    db.storages.Attach(storage);
                    db.storages.Remove(storage);
                    db.SaveChanges();
                }
                return Content("success");
            }
            catch (Exception ex)
            {
                return Content("error");
            }
        }
        
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file1)
        {
            /* 
                @using (Html.BeginForm("Action","Controller",FormMethod.Post, new {enctype="multipart/form-data"}))
                {
                    @Html.Raw(ViewBag.Error)
                    <span>Excel File</span>
                    <input type="file" name="excelfile" />
                    <input type="submit" value="Import">
                }   
             */

            /*if(!isValidContentType(file1.ContentType))
                return Content("content"+file1.ContentType);
            else*/
            var context = new ASPCRUDEntities();
            var jsonSerializer = new JavaScriptSerializer();
            string filename = Path.GetFileName(Server.MapPath(file1.FileName));
            string targetpath = Server.MapPath("~/Doc/");
            string date = DateTime.Now.ToString("ddMMyyyyTHHmmss");
            string new_filePath = targetpath + date + "_" + filename;
            //string new_filePath = targetpath + filename;
            file1.SaveAs(new_filePath);
            OleDbConnection con = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + new_filePath + ";Extended Properties='Excel 12.0 XML;';");
            con.Open();
            //excel storage
            OleDbCommand ExcelCommand = new OleDbCommand("SELECT * FROM [Sheet3$]", con);
            OleDbDataAdapter ExcelAdapter = new OleDbDataAdapter(ExcelCommand);
            var ds = new DataSet();
            ExcelAdapter.Fill(ds, "ExcelTable");
            DataTable dtable = ds.Tables["ExcelTable"];
            List<storage> listStorage = new List<storage>();
            for(int i=0;i<dtable.Rows.Count;i++)
            {
                storage sto = new storage();
                sto.storage_location = dtable.Rows[i][0].ToString();
                sto.warehouse = dtable.Rows[i][1].ToString();
                listStorage.Add(sto);
                //context.storages.Add(sto);
                //context.SaveChanges();
            }
            var json = jsonSerializer.Serialize(listStorage);

            ExcelCommand = new OleDbCommand("SELECT * FROM [Sheet1$]", con);
            ExcelAdapter = new OleDbDataAdapter(ExcelCommand);
            ds = new DataSet();
            ExcelAdapter.Fill(ds, "ExcelTable");
            dtable = ds.Tables["ExcelTable"];
            List<detail_cs> listDetail_cs = new List<detail_cs>();
            for(int i=1;i<3;i++)
            {
                detail_cs detail = new detail_cs();
                detail.exception = dtable.Rows[i][0].ToString();
                detail.material = dtable.Rows[i][1].ToString();
                detail.plant = dtable.Rows[i][2].ToString();
                detail.storage_location = dtable.Rows[i][3].ToString();
                detail.material_description = dtable.Rows[i][4].ToString();
                detail.base_unit_of_measure = dtable.Rows[i][5].ToString();
                detail.batch = dtable.Rows[i][6].ToString();
                detail.unrestricted = Convert.ToInt32(dtable.Rows[i][7]);
                detail.in_quality = Convert.ToInt32(dtable.Rows[i][8]);
                detail.blocked = Convert.ToInt32(dtable.Rows[i][9]);
                detail.total_stock = Convert.ToInt32(dtable.Rows[i][10]);
                detail.market = dtable.Rows[i][11].ToString();
                detail.week = Convert.ToInt32(dtable.Rows[i][12]);
                detail.year = dtable.Rows[i][13].ToString();
                detail.warehouse = dtable.Rows[i][14].ToString();
                listDetail_cs.Add(detail);
                //context.detail_cs.Add(detail);
                //context.SaveChanges();
            }
            /*
                PERHATIKAN BILA ADA YANG KOSONG
                detail_cs detail = new detail_cs();
                detail.exception = dtable.Rows[1][0].ToString();
                detail.material = dtable.Rows[1][1].ToString();
                detail.plant = dtable.Rows[1][2].ToString();
                detail.storage_location = dtable.Rows[1][3].ToString();
                detail.material_description = dtable.Rows[1][4].ToString();
                detail.base_unit_of_measure = dtable.Rows[1][5].ToString();
                detail.batch = dtable.Rows[1][6].ToString();
                detail.unrestricted = Convert.ToInt32(dtable.Rows[1][7]);
                detail.in_quality = Convert.ToInt32(dtable.Rows[1][8]);
                detail.blocked = Convert.ToInt32(dtable.Rows[1][9]);
                detail.total_stock = Convert.ToInt32(dtable.Rows[1][10]);
                detail.market = dtable.Rows[1][11].ToString();
                detail.week = Convert.ToInt32(dtable.Rows[1][12]);
                detail.year = dtable.Rows[1][13].ToString();
                detail.warehouse = dtable.Rows[1][14].ToString();
                detail.id = 1;
                listDetail_cs.Add(detail);
            */
            
            json = jsonSerializer.Serialize(listDetail_cs);
            return Json(new
            {
                Storage = listStorage,
                Detail_cs = listDetail_cs,
                Filepath = filename
            }, JsonRequestBehavior.AllowGet);


            //excel detail_cs
            /*ExcelCommand = new OleDbCommand("SELECT * FROM [Sheet1$]", con);
            ExcelAdapter = new OleDbDataAdapter(ExcelCommand);
            ds = new DataSet();
            ExcelAdapter.Fill(ds, "ExcelTable");
            dtable = ds.Tables["ExcelTable"];
            List<detail_cs> listDetail_cs = new List<detail_cs>();*/
            /*for(int i=0;i<1;i++)
            {
                detail_cs detail = new detail_cs();
                detail.exception = "";
                detail.material = "FA030173.03";
                detail.plant = "ZD11";
                detail.storage_location = "1021";
                detail.material_description = "PETER JACKSON VIRGINIA KS BOX 20";
                detail.base_unit_of_measure = "CS";
                detail.batch = "PI-2017028";
                detail.unrestricted = int.TryParse(dtable.Rows[i][8]);
                detail.in_quality = 8;
                detail.blocked = 11;
                detail.total_stock = 11;
                detail.market = "PACIFIC ISLANDS";
                detail.week = 11;
                detail.year = "PI-2017";
                detail.warehouse = "Lot 17";
                listDetail_cs.Add(detail);

            }
            json = jsonSerializer.Serialize(listDetail_cs);*/
            //string hasil = dtable.Rows[0][8].ToString();
            //return Content(hasil);
            /*
                MENDAPATKAN NAMA SHEETS
                
                dtable = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string[] excelSheets = new string[dtable.Rows.Count];
                int i = 0;
                foreach (DataRow row in dtable.Rows)
                {
                    excelSheets[i] = row["TABLE_NAME"].ToString();
                    i++;
                }

                var excelSheetsJson = jsonSerializer.Serialize(excelSheets);
                return Json(excelSheetsJson);

            */

            /*
              USING DB ENTITIES // DBEntities diganti ASPCRUDEntities
             
              using(DBEntities db = new DBEntities())
             * {
                    db.Customers.Add(model);
                    db.SaveChanges();           --INSERT

                    List<Customer> listCustomer = db.Customers.ToList<Customer>();       --SELECT

                    CustomerID = "ABC"
                    model = db.Customers.Where(x => x.CustomerID == model.CustomerID).FirstOrDefault(); --SELECT WHERE

                    var result = db.Books.SingleOrDefault(b => b.BookNumber == bookNumber); --UPDATE
                    if (result != null)
                    {
                        result.SomeValue = "Some new value";
                        db.SaveChanges();
                    }
                    
                    var employer = new Employ { Id = 1 };
                    ctx.Employ.Attach(employer);
                    ctx.Employ.Remove(employer);
                    ctx.SaveChanges();
               }
             * 
             */

        }
    }
}