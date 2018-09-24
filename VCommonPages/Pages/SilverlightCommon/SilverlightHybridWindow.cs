using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ranorex;

namespace VCommonPages.Pages.SilverlightCommon
{
    //zalozenie jest takie, ze w okienku hybrydowym jest zawsze tabelka.
    //jak bedzie takie okienko, w ktorym nie ma tabelki to nowy konstruktor albo klasa

    public class SilverlightHybridWindow : SilverlightSimpleEditorWindow
    {

        private readonly SimpleEditorFields[] _fields;
        private readonly Form _okno;
        private readonly Container _container;

        public SilverlightTableEditorWindow EditableGrid;

        public SilverlightHybridWindow(WebDocument openWebsite, int? browserId, string idOkna, SimpleEditorFields[] fields, bool znajdzWKontenerze = true,int ktoraTabelka=0)
            : base(openWebsite, browserId, idOkna, fields, znajdzWKontenerze)
        {
            WaitForLoad();
            if (idOkna != "")
                _okno = OpenWebsite.FindSingle<Form>(string.Format(".//form/form[@name='{0}' and @visible='true']", idOkna));
            if (idOkna == "")
                _okno = OpenWebsite.FindSingle<Form>(string.Format(".//form/form[@visible='true']"));
            if (znajdzWKontenerze)
                _container = _okno.FindSingle<Container>(".//container[@automationid='scrollViewer']");
            _fields = fields;

            EditableGrid = new SilverlightTableEditorWindow(OpenWebsite, BrowserId, idOkna,false,false,ktoraTabelka,false, znajdzWKontenerze);

        }
    }

}