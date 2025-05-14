//********************************************************************
// 작   성   자 : 김정열
// 작   성   일 : 2014.01.23
// 모   듈   명 : 품질
// 시 스  템 명 : 유니포인트
// 서브시스템명 : 계약문서현황
// 페 이 지  명 : 계약문서현황
// 프로젝트  명 : P_WP_Z_UNIP_SOPTY_DOC_RPT
//********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using Duzon.Common.Controls;
using Duzon.Common.Forms;
using Duzon.Common.Forms.Help;
using C1.Win.C1FlexGrid;
using Dass.FlexGrid;
using Duzon.ERPU;
using Duzon.ERPU.OLD;
using Duzon.Common.BpControls;
using System.Xml;
using Duzon.Windows.Print;
//using Duzon.ERPU.MF.Common;
using DevExpress.Data.PivotGrid;
using DevExpress.XtraPivotGrid;
using Duzon.Common.Util.Uploader;
//using Excel = Microsoft.Office.Interop.Excel;

using DzHelpFormLib; //추가

namespace sale
{
    public partial class P_SA_Z_THEBORN_USER_SL_MNG : PageBase
  {
        P_SA_Z_THEBORN_USER_SL_MNG_BIZ _biz;
        FreeBinding _free;
        private bool _isMsg = true;//맴버필드
        private PrintHelper _prtHelp = null;
        private FreeBinding _header = new FreeBinding();
        private DataTable S_DT = null;
        private string SAVE_CHK = "N";



        /// <summary>
        /// 사진 정보
        /// </summary>
        DataView _dvPhoto;


        public P_SA_Z_THEBORN_USER_SL_MNG()
        {
            InitializeComponent();

            MainGrids = new FlexGrid[] { _flexH, _flexL };
            _flexH.DetailGrids = new FlexGrid[] { _flexL};
            //DataChanged += new EventHandler(Page_DataChanged);
        }

        #region ♪ 초기화        ♬

        protected override void InitLoad()
        {
            base.InitLoad();
            _biz = new P_SA_Z_THEBORN_USER_SL_MNG_BIZ();
            _free = new FreeBinding();
            InitGrid();
            InitGrid1();
            InitEvent();



            //this._prtHelp = new PrintHelper(this.MainFrameInterface);
            //this._prtHelp.OnPrintEventProc += new PrintHelper.PrintEventHandler(_prtHelp_OnPrintEventProc);
            //this._prtHelp.OnPrintDialogEventProc += new PrintHelper.PrintDialogEventHandler(_prtHelp_OnPrintDialogEventProc);

        }

        private void InitGrid()
        {
            _flexH.BeginSetting(1, 1, false);

            _flexH.SetCol("S", "S", 50, true, CheckTypeEnum.Y_N);
            _flexH.SetCol("USER_TP", "사용자유형", 110, 20, false);
            _flexH.SetCol("USER_ID", "사용자ID", 150, 100, false);
            _flexH.SetCol("USER_NM", "닉네임", 180, 500, false);


            _flexH.SetCol("PLANT_CD", "공장", 0, 20, false);

            _flexH.Cols["PLANT_CD"].Visible = false;
            _flexH.SetDummyColumn("S");
            

            _flexH.SettingVersion = "1.0.0.2";
            _flexH.EndSetting(GridStyleEnum.Green, AllowSortingEnum.SingleColumn, SumPositionEnum.None);

        }

        private void InitGrid1()
        {
            _flexL.BeginSetting(1, 1, false);

            _flexL.SetCol("S", "S", 50, true, CheckTypeEnum.Y_N);
            _flexL.SetCol("CD_SL", "창고코드", 120, 20, false);
            _flexL.SetCol("NM_SL", "창고명", 180, 500, false);
            _flexL.SetCol("USER_ID", "사용자ID", 0, 20, false);


            _flexL.SettingVersion = "1.0.0.1";
            _flexL.EndSetting(GridStyleEnum.Green, AllowSortingEnum.SingleColumn, SumPositionEnum.None);

        }

        private void InitEvent()
        {
            _flexH.AfterRowChange += new RangeEventHandler(_flexH_AfterRowChange);
            btn적용.Click += new EventHandler(btn적용_Click);
        }

        protected override void InitPaint()
        {
            base.InitPaint();

            InitControl();
            
           
        }

        private void InitControl()
        {

            //관련부문[0], 관련부문[1], 제안유형[2], 최종등급[3]
            DataSet _ds = GetComboData("S;QU_Z000001", "N;QU_Z000001", "N;QU_Z000002", "S;QU_Z000003");

            DataTable _dt_usertp = _biz.SEARCH_CLASS();
            
            //관련부문
            CBO_USER_TP.DataSource = _dt_usertp;
            CBO_USER_TP.DisplayMember = "NAME";
            CBO_USER_TP.ValueMember = "CODE";


            //DTP_등록기간.StartDateToString = MainFrameInterface.GetStringFirstDayInMonth;
            //DTP_등록기간.EndDateToString = MainFrameInterface.GetStringToday;

            //DT_FR.Text = MainFrameInterface.GetDateTimeToday().ToString();


        }

        protected override bool IsChanged()
        {
            if (base.IsChanged()) return true;

            return 헤더변경여부;
        }

        #endregion

        #region ♪ 메인 버튼     ♬

        protected override bool BeforeSearch()
        {
            if (!base.BeforeSearch()) return false;
            return true;
        }

        public override void OnToolBarSearchButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (!base.BeforeSearch()) return;


                object[] param = new object[6];
                param[0] = this.LoginInfo.CompanyCode;                      //회사코드  
                param[1] = D.GetString(CBO_USER_TP.SelectedValue);                          //공장  
                param[2] = D.GetString(TXT_SEARCH.Text);     //기준일자FR
                

                MsgControl.ShowMsg("자료 조회중입니다. 잠시만 기다려주세요.");

                if (_flexL.DataTable != null)
                {
                    _flexL.Binding = _flexL.DataTable.Clone();
                }

                DataSet ds = _biz.Search(param);

                _flexH.Binding = ds.Tables[0];

                MsgControl.CloseMsg();

                if (!_flexH.HasNormalRow)
                {
                    this.ShowMessage(공통메세지.조건에해당하는내용이없습니다);
                }
                else
                {
                    string STR_FILTER = "USER_ID = '" + D.GetString(_flexH[_flexH.Row, "USER_ID"]) + "'";

                    _flexL.Binding = ds.Tables[1];

                    _flexL.RowFilter = STR_FILTER;
                    //SetGridSubTotal1();
                }


            }
            catch (Exception ex)
            {
                MsgControl.CloseMsg();
                MsgEnd(ex);
            }
            finally
            {
                MsgControl.CloseMsg();
            }
        }

        protected override bool BeforeAdd()
        {
            if (!base.BeforeAdd()) return false;

            // 메인의 추가버튼 클릭 시 변경된 사항이 있으면 메세지 처리 후 추가..
            if (!MsgAndSave(PageActionMode.Search)) return false;

            return true;
        }

        public override void OnToolBarAddButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (!BeforeAdd()) return;

                //_flex.Rows.Add();

                //_flex.Row = _flex.Rows.Count - 1;

               
            }
            catch (Exception ex)
            {
                MsgEnd(ex);
            }
        }

        protected override bool BeforeDelete()
        {
            //if (!base.BeforeDelete()) return false;

            //DialogResult result = ShowMessage(공통메세지.자료를삭제하시겠습니까, PageName);
            //if (result != DialogResult.Yes) return false;
            return true;
        }

        public override void OnToolBarDeleteButtonClicked(object sender, EventArgs e)
        {
            try
            {
                //if (!_flex.HasNormalRow) return;


                //_flex.Rows.Remove(_flex.Row);
               

            }
            catch (Exception ex)
            {
                MsgEnd(ex);
            }
        }

        protected override bool BeforeSave()
        {
            if (!HeaderCheck(0)) return false;

            if (!Verify()) return false;    // 그리드 체크

            return true;
        }
        
        public override void OnToolBarSaveButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (!BeforeSave()) return;


                if (MsgAndSave(PageActionMode.Save))
                {
                    ShowMessage(PageResultMode.SaveGood);
                    _flexL.AcceptChanges();
                    //OnToolBarSearchButtonClicked(null, null);
                }

            }
            catch (Exception ex)
            {
                MsgEnd(ex);
            }
        }

        public override void OnToolBarPrintButtonClicked(object sender, EventArgs e)
        {
            try
            {
                //string CODE = string.Empty;
                //string NAME = string.Empty;
                ////switch (tabControlExt1.SelectedIndex.ToString())
                ////{
                ////    case "0":
                ////        flex = _flex_매출;
                ////        CODE = "R_SA_IV_LEDGER_1";
                ////        NAME = "매출기준";
                ////        break;
                ////    case "1":
                ////        flex = _flex_미매출;
                ////        CODE = "R_SA_IV_LEDGER_2";
                ////        NAME = "미매출기준";
                ////        break;

                ////    case "2":
                ////        flex = _flex_매출상세;
                ////        CODE = "R_SA_IV_LEDGER_3";
                ////        NAME = "매출기준-상세";
                ////        break;

                ////    case "3":
                ////        flex = _flex_미매출상세;
                ////        CODE = "R_SA_IV_LEDGER_4";
                ////        NAME = "미매출기준-상세";
                ////        break;
                ////}
                //if (tabControlExt1.SelectedTab.Name.ToString() == "tabPage1")
                //{
                //    if (!_flex1.HasNormalRow) return;

                //    ReportHelper rpt = new ReportHelper("R_QU_Z_ANJUN_SUGGEST_RPT1", "제안현황(제안자별)");
                //    //rpt.SetData("조회일", dp_조회일from.MaskEditBox.FormattedText + " ~ " + dp_조회일to.MaskEditBox.FormattedText);

                //    //string 영업그룹 = bp_영업그룹.SelectedText;
                //    //if (bp_영업그룹.Count > 1)
                //    //{
                //    //    영업그룹 += "외 " + D.GetString(bp_영업그룹.Count - 1) + "건";
                //    //}

                //    //System.Diagnostics.Debug.WriteLine(영업그룹);

                //    //rpt.SetData("영업그룹", 영업그룹);
                //    //rpt.SetData("수주담당자", bp_담당자.CodeName);
                //    //rpt.SetData("거래처", bp_거래처.CodeName);
                //    rpt.SetDataTable(_flex1.DataTable);
                //    rpt.Print();
                //}
                //else
                //{
                //    if (!_flex2.HasNormalRow) return;

                //    ReportHelper rpt = new ReportHelper("R_QU_Z_ANJUN_SUGGEST_RPT2", "제안현황(등급별)");
                //    //rpt.SetData("조회일", dp_조회일from.MaskEditBox.FormattedText + " ~ " + dp_조회일to.MaskEditBox.FormattedText);

                //    //string 영업그룹 = bp_영업그룹.SelectedText;
                //    //if (bp_영업그룹.Count > 1)
                //    //{
                //    //    영업그룹 += "외 " + D.GetString(bp_영업그룹.Count - 1) + "건";
                //    //}

                //    //System.Diagnostics.Debug.WriteLine(영업그룹);

                //    //rpt.SetData("영업그룹", 영업그룹);
                //    //rpt.SetData("수주담당자", bp_담당자.CodeName);
                //    //rpt.SetData("거래처", bp_거래처.CodeName);
                //    rpt.SetDataTable(_flex2.DataTable);
                //    rpt.Print();
                //}
            }
            catch (Exception ex)
            {
                MsgEnd(ex);
            }
        }
       
        #endregion

        #region ♪ 화면 내 버튼  ♬

        void btn적용_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_flexH.HasNormalRow) return;

                if (_flexH.GetCheckedRows("S") == null)
                {
                    this.ShowMessage("체크된 아이디가 없습니다. 창고권한을 줄 아이디를 체크해 주세요");
                    return;
                }


                string STR_MSG = "선택한 아이디(" + D.GetString(_flexH[_flexH.Row, "USER_ID"]) + ")가 가지고있는 권한을 체크된아이디(" + D.GetString(_flexH.GetCheckedRows("S").Rows.Count) + ")에게 적용하시겠습니까?";

                DialogResult dResult = ShowMessage(STR_MSG, "QY2");
                if (dResult != DialogResult.Yes) return;

                MsgControl.ShowMsg("창고권한 적용중입니다. 잠시만 기다려주세요.");

                _flexL.DataTable.Select("USER_ID = '" + D.GetString(_flexH[_flexH.Row, "USER_ID"]) + "' AND ISNULL(S, 'N') = 'Y' ");

                DataRow[] DRL = _flexL.DataTable.Select("USER_ID = '" + D.GetString(_flexH[_flexH.Row, "USER_ID"]) + "' AND ISNULL(S, 'N') = 'Y' ");
                DataRow[] DRL1;
                DataRow[] DRL2;
                DataTable _dt_sl = _flexL.DataTable.Clone();

                if (DRL.Length > 0)
                {
                    foreach (DataRow dr in DRL)
                    {
                        _dt_sl.ImportRow(dr);
                    }
                }
                else
                {
                    this.ShowMessage("선택한 아이디(" + D.GetString(_flexH[_flexH.Row, "USER_ID"]) +  ")의 창고 권한이 없습니다.");
                    return;
                }

                foreach (DataRow dtuser in _flexH.GetCheckedRows("S").Rows)
                {
                    if (D.GetString(dtuser["USER_ID"]) == D.GetString(_flexH[_flexH.Row, "USER_ID"])) continue;

                    DRL1 = _flexL.DataTable.Select("USER_ID = '" + D.GetString(dtuser["USER_ID"]) + "'");
                    if (DRL1.Length > 0)
                    {
                        foreach (DataRow dr in DRL1)
                        {
                            DRL2 = _dt_sl.Select("CD_SL = '" + dr["CD_SL"] + "'");
                            if (DRL2.Length > 0)
                            {
                                dr["S"] = "Y";
                            }
                            else
                            {
                                dr["S"] = "N";
                            }
                        }
                    }
                }
                MsgControl.CloseMsg();
                this.ShowMessage("창고권한 적용이 완료되었습니다. 저장을 누르면 최종 적용됩니다.");


            }
            catch (Exception ex)
            {
                MsgControl.CloseMsg();
                MsgEnd(ex);
            }
        }
        

        #endregion

        #region ♪ 저장 관련     ♬

        bool HeaderCheck(int pChk)
        {

            // 헤더 데이터 필수 등록 체크

            return true;
        }

        protected override bool SaveData()
        {
            if (!base.SaveData()) return false;


            if (_flexL.GetChanges() == null || _flexL.GetChanges().Rows.Count <= 0) return false;


            _biz.Save(_flexL.GetChanges());


            return true;
        }

        #endregion

        #region ♪ 그리드 이벤트 ♬

        void _flexH_AfterRowChange(object sender, RangeEventArgs e)
        {
            try
            {
                if (!_flexH.HasNormalRow) return;

                string STR_FILTER = "USER_ID = '" + D.GetString(_flexH[_flexH.Row, "USER_ID"]) + "'";


                _flexL.RowFilter = STR_FILTER;
            }
            catch (Exception ex)
            {
                MsgEnd(ex);
            }
        }
       
        #endregion

        #region ♪ 기타 이벤트

        #region Control_QueryBefore
        void Control_QueryBefore(object sender, BpQueryArgs e)
        {
            switch (e.ControlName)
            {


                case "bpcCD_ITEM":

                    e.HelpParam.P09_CD_PLANT = D.GetString(LoginInfo.CdPlant);//공장코드

                    break;

                case "CTX_CD_SL":

                    e.HelpParam.P09_CD_PLANT = D.GetString(LoginInfo.CdPlant);//공장코드

                    break;
            }
        }
        #endregion

        #region -> Control_QueryAfter

        private void Control_QueryAfter(object sender, Duzon.Common.BpControls.BpQueryArgs e)
        {
            if (e.DialogResult == DialogResult.Cancel)
                return;

            switch (e.ControlName)
            {
                case "BP_창고":
                    //BP_창고.CodeName = e.HelpReturn.Rows[0]["NM_SL"].ToString();
                    //BP_창고.CodeValue = e.HelpReturn.Rows[0]["CD_SL"].ToString();

                    break;
            }
        }

        #endregion

        #endregion

        #region ♪ 기타 메서드   ♬
        #region -> 컨트롤 초기화(0)
        //private void SetControlClear(Control ctrls)
        //{

        //    foreach (Control ctrlsPanel in ((FlexibleRoundedCornerBox)ctrls).Controls)
        //    {
        //        // **** 커런시 컨트롤
        //        if (ctrlsPanel.GetType().Name == "CurrencyTextBox")
        //        {
        //            ((Duzon.Common.Controls.CurrencyTextBox)ctrlsPanel).DecimalValue = 0;
        //        }
        //        // **** 마스크에디터
        //        else if (ctrlsPanel.GetType().Name == "MaskedEditBox")
        //        {
        //            ((Duzon.Common.Controls.MaskedEditBox)ctrlsPanel).Text = string.Empty;
        //        }
        //        // **** TextBox
        //        else if (ctrlsPanel.GetType().Name == "TextBoxExt")
        //        {
        //            ((TextBoxExt)ctrlsPanel).Text = string.Empty;
        //        }
        //        // **** DatePicker
        //        else if (ctrlsPanel.GetType().Name == "DatePicker")
        //        {
        //            ((DatePicker)ctrlsPanel).Text = string.Empty;
        //        }
        //        // **** BpCodeTextBox
        //        else if (ctrlsPanel.GetType().Name == "BpCodeTextBox")
        //        {
        //            ((Duzon.Common.BpControls.BpCodeTextBox)ctrlsPanel).Text = string.Empty;
        //            ((Duzon.Common.BpControls.BpCodeTextBox)ctrlsPanel).CodeName = string.Empty;
        //            ((Duzon.Common.BpControls.BpCodeTextBox)ctrlsPanel).CodeValue = string.Empty;
        //        }
        //        // **** BpCodeNTextBox
        //        else if (ctrlsPanel.GetType().Name == "BpCodeNTextBox")
        //        {
        //            ((Duzon.Common.BpControls.BpCodeNTextBox)ctrlsPanel).Text = string.Empty;
        //            ((Duzon.Common.BpControls.BpCodeNTextBox)ctrlsPanel).CodeName = string.Empty;
        //            ((Duzon.Common.BpControls.BpCodeNTextBox)ctrlsPanel).CodeValue = string.Empty;
        //        }
        //        // 콤보박스
        //        else if (ctrlsPanel.GetType().Name == "DropDownComboBox")
        //        {
        //            ((DropDownComboBox)ctrlsPanel).SelectedIndex = -1;
        //        }
        //        else if (ctrlsPanel.GetType().Name == "PanelExt")
        //            SetControlClear(ctrlsPanel);
        //    }
        //}
        
        private void SetControlClear(Control ctrls)
        {
            foreach (Control ctrlsPanel in ((PanelExt)ctrls).Controls)
            {
                // **** 커런시 컨트롤
                if (ctrlsPanel.GetType().Name == "CurrencyTextBox")
                {
                    ((Duzon.Common.Controls.CurrencyTextBox)ctrlsPanel).DecimalValue = 0;
                }
                // **** 마스크에디터
                else if (ctrlsPanel.GetType().Name == "MaskedEditBox")
                {
                    ((Duzon.Common.Controls.MaskedEditBox)ctrlsPanel).Text = string.Empty;
                }
                // **** TextBox
                else if (ctrlsPanel.GetType().Name == "TextBoxExt")
                {
                    ((TextBoxExt)ctrlsPanel).Text = string.Empty;
                }
                // **** DatePicker
                else if (ctrlsPanel.GetType().Name == "DatePicker")
                {
                    ((DatePicker)ctrlsPanel).Text = string.Empty;
                }
                // **** BpCodeTextBox
                else if (ctrlsPanel.GetType().Name == "BpCodeTextBox")
                {
                    ((Duzon.Common.BpControls.BpCodeTextBox)ctrlsPanel).Text = string.Empty;
                    ((Duzon.Common.BpControls.BpCodeTextBox)ctrlsPanel).CodeName = string.Empty;
                    ((Duzon.Common.BpControls.BpCodeTextBox)ctrlsPanel).CodeValue = string.Empty;
                }
                // **** BpCodeNTextBox
                else if (ctrlsPanel.GetType().Name == "BpCodeNTextBox")
                {
                    ((Duzon.Common.BpControls.BpCodeNTextBox)ctrlsPanel).Text = string.Empty;
                    ((Duzon.Common.BpControls.BpCodeNTextBox)ctrlsPanel).CodeName = string.Empty;
                    ((Duzon.Common.BpControls.BpCodeNTextBox)ctrlsPanel).CodeValue = string.Empty;
                }
                // 콤보박스
                else if (ctrlsPanel.GetType().Name == "DropDownComboBox")
                {
                    ((DropDownComboBox)ctrlsPanel).SelectedIndex = -1;
                }
                else if (ctrlsPanel.GetType().Name == "PanelExt")
                    SetControlClear(ctrlsPanel);
            }
        }
        
        #endregion -> 컨트롤 초기화(0)


        #endregion

        #region ♪ 속성          ♬


        //private bool DT_등록기간 { get { return Checker.IsValid(DTP_등록기간, true, DD("기준일자")); } }
        //private bool DT_비교기간 { get { return Checker.IsValid(DTP_비교기간, true, DD("비교기간")); } }

        private bool 헤더변경여부
        {
            get
            {
                bool bChange = false;

                bChange = _free.GetChanges() != null ? true : false;
                DataTable dt = _free.GetChanges();

                //if (bChange && _free.JobMode == JobModeEnum.추가후수정 && !_flex.HasNormalRow)
                //    bChange = false;

                return bChange;
            }
        }
        #endregion


    }
}