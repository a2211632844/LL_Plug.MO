using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.NotePrint;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace LL_Plug.MO
{
    [HotUpdate]
    public class CreateBarCode : AbstractBillPlugIn
    {
        public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterEntryBarItemClick(e);
            //如果点击条码生成打印按钮
            if (e.BarItemKey == "tbCreateBarcode")
            {
                int idx = this.Model.GetEntryCurrentRowIndex("FTreeEntity");

                //生产订单日期 生产订单号 单位 产品编码 产品名称 规格型号 生产订单数量
                string scddDate = this.View.Model.GetValue("FDATE", idx).ToString();//生产订单日期(2021-1-5)
                string Fid = this.View.Model.GetValue("FBILLNO", idx).ToString();//单据编号(MO20210105-1483)

                DynamicObject funit = this.Model.GetValue("FUNITID", idx) as DynamicObject;
                string unit = funit[0].ToString();//单位(盒)
                string unitname = funit["Number"].ToString();

                DynamicObject fmaterialId = this.Model.GetValue("FMATERIALID", idx) as DynamicObject;
                string FMaterialId = fmaterialId["ID"].ToString();//物料num237054
                string FMATERIALID = fmaterialId["Name"].ToString();//物料name冷冻榴莲肉
                string FMATERIALIDNumber = fmaterialId["Number"].ToString();//物料name冷冻榴莲肉
                string GGXH = fmaterialId["Specification"].ToString(); //规格型号
                string qty = this.Model.GetValue("FQTY", idx).ToString();//生产订单数量
                string FDeliveryDate = this.Model.GetValue("F_YHRQ", idx)==null?"": this.Model.GetValue("F_YHRQ", idx).ToString();//要货日期
                string KH = "";
                if (this.Model.GetValue("F_LLL_KH").IsNullOrEmptyOrWhiteSpace() == false) 
                {
                    DynamicObject dy_KH = this.Model.GetValue("F_LLL_KH") as DynamicObject;
                    KH = dy_KH["Name"].ToString();
                }
                string JHDDRQ = Convert.ToDateTime(this.Model.GetValue("FPlanStartDate")).ToString("yyyy-MM-dd");

                string BOMID = "";
                string BOMNUMBER = "";
                string BOMNAME = "";
                if (this.Model.GetValue("FBOMID", idx).IsNullOrEmptyOrWhiteSpace()==false) 
                {
                    DynamicObject dy_Bom = this.Model.GetValue("FBOMID", idx) as DynamicObject;
                    BOMID = dy_Bom["ID"].ToString();
                    BOMNUMBER = dy_Bom["Number"].ToString();
                    BOMNAME = dy_Bom["Name"].ToString();
                }
                string F_LLL_KHPH = this.Model.GetValue("F_LLL_KHPH", idx).ToString();
                string TGFS = "";
                if (this.Model.GetValue("FAUXPROPID", idx).IsNullOrEmptyOrWhiteSpace()==false) 
                {
                    DynamicObject dy_TGFS = this.Model.GetValue("FAUXPROPID", idx) as DynamicObject;
                    TGFS = dy_TGFS[2].ToString();
                }
                string F_LLL_HTH = "";
                if (this.Model.GetValue("F_LLL_HTH", idx).IsNullOrEmptyOrWhiteSpace()==false) 
                {
                    F_LLL_HTH = this.Model.GetValue("F_LLL_HTH", idx).ToString();
                }
                string F_LLL_KHMaterialID = "";
                string F_LLL_KHMaterialName = "";
                if (this.Model.GetValue("F_LLL_Base",idx).IsNullOrEmptyOrWhiteSpace()==false) 
                {
                    DynamicObject dy_khwlbm = this.Model.GetValue("F_LLL_Base", idx) as DynamicObject;
                    F_LLL_KHMaterialID = dy_khwlbm[0].ToString();
                    F_LLL_KHMaterialName = dy_khwlbm["Name"].ToString();
                    //F_LLL_KHMaterialID = this.Model.GetValue("F_LLL_Base", idx).ToString();
                }
                string FOrderNo = "";
                //string xx = this.Model.GetValue("F_SHDDBH", idx).ToString();
                if (this.Model.GetValue("F_SHDDBH", idx).ToString().Trim()!=null) 
                {
                    FOrderNo = this.Model.GetValue("F_SHDDBH", idx).ToString();//销售订单号
                }

                string F_LLL_BC = ""; //班次
                if (this.Model.GetValue("F_LLL_BC").IsNullOrEmptyOrWhiteSpace()==false) 
                {
                    DynamicObject dybc = this.Model.GetValue("F_LLL_BC") as DynamicObject;
                    F_LLL_BC = dybc[0].ToString();
                }
                string F_LLL_JT = "";
                if (this.Model.GetValue("F_LLL_JT").IsNullOrEmptyOrWhiteSpace() == false)
                {
                    DynamicObject dybc = this.Model.GetValue("F_LLL_JT") as DynamicObject;
                    F_LLL_JT = dybc[0].ToString();
                }
                string F_LLL_SYM = "";
                if (this.Model.GetValue("F_LLL_SYM").IsNullOrEmptyOrWhiteSpace() == false) 
                {
                    F_LLL_SYM = this.Model.GetValue("F_LLL_SYM", idx).ToString();//朔源码
                }
                string HTCPPH = "";
                if (this.Model.GetValue("F_LLL_INTEGER4").IsNullOrEmptyOrWhiteSpace()==false) 
                {
                    HTCPPH = this.Model.GetValue("F_LLL_INTEGER4", idx).ToString();//虎头成品批号
                }
                string FCust = string.Empty;
                try
                {
                    DynamicObject custobj = this.Model.GetValue("FCust", idx) as DynamicObject;
                    if (custobj != null)
                    {
                        FCust = custobj["ID"].ToString();//客户
                    }
                    else
                    {
                        FCust = "0";
                    }
                }catch(Exception ex)
                {
                    FCust = "0";
                }
                string FOrderQty = this.Model.GetValue("FOrderQty", idx).ToString();//销售订单数量
                //销售订单号
                string XSDDH = "";
                if (this.Model.GetValue("F_LLL_TEXT2").IsNullOrEmptyOrWhiteSpace() == false) 
                {
                     XSDDH = this.Model.GetValue("F_LLL_TEXT2").ToString();
                }

                //客户合同号
                string KHHTH = "";
                if (this.Model.GetValue("F_LLL_TEXT5").IsNullOrEmptyOrWhiteSpace() == false)
                {
                    KHHTH = this.Model.GetValue("F_LLL_TEXT5").ToString();
                }


                //打开动态表单
                DynamicFormShowParameter param = new DynamicFormShowParameter();
                param.FormId = "kd40ad6fb6d094d19ac1b0c68d37ad082";//条码批量打印 动态表单

                //动态表单id唯一标识
                param.CustomParams.Add("SCDDDATE", scddDate); //生产订单日期
                param.CustomParams.Add("FID", Fid);//生产订单号 

                var rowData = this.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("FTreeEntity"), idx); ;
                param.CustomParams.Add("FSeq", (rowData["Seq"]).ToString());//当前行号
                param.CustomParams.Add("UNIT", unit);//  单位
                param.CustomParams.Add("FMATERIALID", FMaterialId);//产品编码
                param.CustomParams.Add("FMATERIALIDNAME", FMATERIALID);//产品名称
                param.CustomParams.Add("FMATERIALIDNumber", FMATERIALIDNumber);//生产订单数量
                param.CustomParams.Add("GGXH", GGXH);//规格型号
                param.CustomParams.Add("QTY", qty);//生产订单数量
                param.CustomParams.Add("UNITNAME", unitname);//单位
                param.CustomParams.Add("FDeliveryDate", FDeliveryDate);//要货日期
                param.CustomParams.Add("FOrderNo", FOrderNo);//销售订单号
                param.CustomParams.Add("FCust", FCust);//客户
                param.CustomParams.Add("FOrderQty", FOrderQty);//销售订单数量
                param.CustomParams.Add("BOMID",BOMID);//BOMID
                param.CustomParams.Add("BOMNUMBER", BOMNUMBER);
                param.CustomParams.Add("BOMNAME", BOMNAME);
                param.CustomParams.Add("F_LLL_KHPH", F_LLL_KHPH);//F_LLL_KHPH客户批号
                param.CustomParams.Add("TGFS", TGFS);//TGFS套管方式
                param.CustomParams.Add("F_LLL_HTH", F_LLL_HTH);//合同号
                param.CustomParams.Add("F_LLL_KHMaterialID", F_LLL_KHMaterialID);//客户物料编码
                param.CustomParams.Add("F_LLL_KHMaterialName", F_LLL_KHMaterialName);//客户物料名称
                param.CustomParams.Add("F_LLL_KH",KH);
                param.CustomParams.Add("F_LLL_BC", F_LLL_BC);//班次
                param.CustomParams.Add("F_LLL_JT", F_LLL_JT);//机台
                param.CustomParams.Add("F_LLL_JHDDRQ", JHDDRQ);
                param.CustomParams.Add("F_LLL_SYM",F_LLL_SYM);
                param.CustomParams.Add("HTCPPH", HTCPPH);
                param.CustomParams.Add("KHHTH", KHHTH);//客户合同号
                param.CustomParams.Add("XSDDH", XSDDH);//销售订单号
                //throw new Exception(scddDate + Fid + unit + FMaterialId + FMATERIALID + qty);
                //传送参数到动态表单  
                //DynamicObject dy_TypeID = this.Model.GetValue("") as DynamicObject;
                //string TypeID = dy_TypeID[0].ToString();
                this.View.ShowForm(param, new Action<FormResult>((formResult) =>
                {
                    //页面赋值
                    if (formResult != null && formResult.ReturnData != null)
                    {
                        string sql = $@"select FPRINT as F_ora_DefaultPrinter from T_SEC_USER
                                        where FUSERID = {this.Context.UserId}";
                        var sqlResult = DBServiceHelper.ExecuteDynamicObject(Context, sql);
                        //正式账套：ad29504a-a54c-41f3-9aa3-d0501e688e72
                        //测试账套：7ea88099-9273-4ee3-9312-747453c7b6fc
                        //List<string> list1 = (List<string>)formResult.ReturnData;
                        //string ss1 = list1[0].ToString().Trim();
                        //string ss2 = list1[1].ToString().Trim();

                        Dictionary<string, List<string>> diclist =(Dictionary<string, List<string>>)formResult.ReturnData;
                        Dictionary<string, List<string>>.KeyCollection keys = diclist.Keys;
                        string templateid = "";
                        foreach (string tempkey in keys)
                        {
                            templateid = tempkey;
                        }
                        List<string> list = diclist[templateid];


                        string printerName = string.Empty;
                        //List<string> list2 = new List<string>();
                        //list2.Add(ss1);
                        if (sqlResult.Count > 0 && !sqlResult[0]["F_ora_DefaultPrinter"].IsNullOrEmptyOrWhiteSpace())
                        {
                            printerName = sqlResult[0]["F_ora_DefaultPrinter"].ToString();
                        }
                        //PrintView(list2, ss2 , printerName);
                        PrintView(list, templateid, printerName);
                    }
                    else
                    {
                        //this.View.ShowErrMessage("条码值为空");
                    }
                }
                ));
            }

        }

        /// <summary>
        /// 设置套打模板并打开预览界面
        /// </summary>
        /// <param name="billIds">单据内码</param>
        /// <param name="templateId">套打模板标识</param>
        /// <param name="printerName">打印机名称或地址</param>
        private void PrintView(List<string> billIds, string templateId, string printerName)
        {
            Queue<List<PrintJob>> printJobsQueue = new Queue<List<Kingdee.BOS.Core.NotePrint.PrintJob>>();
            PrintJob printJob = new PrintJob();
            
            // 业务对象标识
            printJob.FormId = "k88c57702012242e6b81671fe123aa125";
            
            printJob.PrintJobItems = new List<PrintJobItem>();

            foreach (string id in billIds)
            {
                printJob.PrintJobItems.Add(new PrintJobItem(id, templateId));
            }

            printJobsQueue.Enqueue(new List<PrintJob>() { printJob });
            var currPringJob = printJobsQueue.Dequeue();
            string printJobsKey = Guid.NewGuid().ToString();
            this.View.Session[printJobsKey] = currPringJob;


            //string key = Guid.NewGuid().ToString();
            //this.View.Session[key] = printInfoList;
            JSONObject jsonObj = new JSONObject();
            jsonObj.Put("pageID", this.View.PageId);
            jsonObj.Put("printJobId", printJobsKey);
            //jsonObj.Put("action", "print");//预览--printType赋值为"preview";打印--printType赋值为"print"
            jsonObj.Put("action", "preview");//预览--printType赋值为"preview";打印--printType赋值为"print"
            //string action = "printPreview";
            if (!string.IsNullOrEmpty(printerName))
            {
                jsonObj.Put("printBarName", printerName);
                jsonObj.Put("printerAddress", printerName);
            }
            this.View.AddAction(Kingdee.BOS.Core.Const.JSAction.printPreview, jsonObj);

        }
    }
}
