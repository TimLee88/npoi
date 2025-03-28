﻿/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */

using TestCases.SS.UserModel;
using System;
using NUnit.Framework;using NUnit.Framework.Legacy;
using NPOI.SS.UserModel;
using NPOI.OpenXml4Net.OPC;
using NPOI.HSSF.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using NPOI.XSSF;

namespace TestCases.XSSF.UserModel
{
    [TestFixture]
    public class TestXSSFHyperlink : BaseTestHyperlink
    {
        public TestXSSFHyperlink()
            : base(XSSFITestDataProvider.instance)
        {

        }

        [SetUp]
        public void SetUp()
        {
            // Use system out logger
            Environment.SetEnvironmentVariable(
                    "NPOI.util.POILogger",
                    "NPOI.util.SystemOutLogger"
            );
        }
        [Test]
        public void TestLoadExisting()
        {
            XSSFWorkbook workbook = XSSFTestDataSamples.OpenSampleWorkbook("WithMoreVariousData.xlsx");
            ClassicAssert.AreEqual(3, workbook.NumberOfSheets);

            XSSFSheet sheet = (XSSFSheet)workbook.GetSheetAt(0);

            // Check the hyperlinks
            ClassicAssert.AreEqual(4, sheet.NumHyperlinks);
            doTestHyperlinkContents(sheet);
        }
        [Test]
        public void TestCreate()
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            XSSFSheet sheet = workbook.CreateSheet() as XSSFSheet;
            XSSFRow row = sheet.CreateRow(0) as XSSFRow;
            XSSFCreationHelper CreateHelper = workbook.GetCreationHelper() as XSSFCreationHelper;

            String[] urls = {
                "http://apache.org/",
                "www.apache.org",
                "/temp",
                "file:///c:/temp",
                "http://apache.org/default.php?s=isTramsformed&submit=Search&la=*&li=*",
                "Test/測試.pdf",
                "https://somedomain.com/Тест А.pdf"};
            for (int i = 0; i < urls.Length; i++)
            {
                String s = urls[i];
                XSSFHyperlink link = CreateHelper.CreateHyperlink(HyperlinkType.Url) as XSSFHyperlink;
                link.Address=(s);

                XSSFCell cell = row.CreateCell(i) as XSSFCell;
                cell.Hyperlink=(link);
            }
            workbook = XSSFTestDataSamples.WriteOutAndReadBack(workbook) as XSSFWorkbook;
            sheet = workbook.GetSheetAt(0) as XSSFSheet;
            PackageRelationshipCollection rels = sheet.GetPackagePart().Relationships;
            ClassicAssert.AreEqual(urls.Length, rels.Size);
            for (int i = 0; i < rels.Size; i++)
            {
                PackageRelationship rel = rels.GetRelationship(i);
                if (rel.TargetUri.IsAbsoluteUri&&rel.TargetUri.IsFile)
                    ClassicAssert.AreEqual(urls[i].Replace("file:///","").Replace("/","\\"),rel.TargetUri.LocalPath);
                else
                    // there should be a relationship for each URL
                    ClassicAssert.AreEqual(urls[i], rel.TargetUri.ToString());
            }

            // Bugzilla 53041: Hyperlink relations are duplicated when saving XSSF file
            workbook = XSSFTestDataSamples.WriteOutAndReadBack(workbook) as XSSFWorkbook;
            sheet = workbook.GetSheetAt(0) as XSSFSheet;
            rels = sheet.GetPackagePart().Relationships;
            ClassicAssert.AreEqual(urls.Length, rels.Size);
            for (int i = 0; i < rels.Size; i++)
            {
                PackageRelationship rel = rels.GetRelationship(i);
                if (rel.TargetUri.IsAbsoluteUri && rel.TargetUri.IsFile)
                    ClassicAssert.AreEqual(urls[i].Replace("file:///", "").Replace("/", "\\"), rel.TargetUri.LocalPath);
                else
                    // there should be a relationship for each URL
                    ClassicAssert.AreEqual(urls[i], rel.TargetUri.ToString());
            }
        }
        [Ignore("the Urls are valid")]
        public void TestInvalidURLs()
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            XSSFCreationHelper CreateHelper = workbook.GetCreationHelper() as XSSFCreationHelper;

            String[] invalidURLs = {
                //"http:\\apache.org",
                //"www.apache .org",
                "c:\\temp",
                "\\poi"};
            foreach (String s in invalidURLs)
            {
                try
                {
                    CreateHelper.CreateHyperlink(HyperlinkType.Url).Address = (s);
                    Assert.Fail("expected ArgumentException: " + s);
                }
                catch (ArgumentException)
                {
                }
            }
        }

        [Test]
        public void TestLoadSave()
        {
            XSSFWorkbook workbook = XSSFTestDataSamples.OpenSampleWorkbook("WithMoreVariousData.xlsx");
            ICreationHelper CreateHelper = workbook.GetCreationHelper();
            ClassicAssert.AreEqual(3, workbook.NumberOfSheets);
            XSSFSheet sheet = (XSSFSheet)workbook.GetSheetAt(0);

            // Check hyperlinks
            ClassicAssert.AreEqual(4, sheet.NumHyperlinks);
            doTestHyperlinkContents(sheet);


            // Write out, and check

            // Load up again, check all links still there
            XSSFWorkbook wb2 = (XSSFWorkbook)XSSFTestDataSamples.WriteOutAndReadBack(workbook);
            ClassicAssert.AreEqual(3, wb2.NumberOfSheets);
            ClassicAssert.IsNotNull(wb2.GetSheetAt(0));
            ClassicAssert.IsNotNull(wb2.GetSheetAt(1));
            ClassicAssert.IsNotNull(wb2.GetSheetAt(2));

            sheet = (XSSFSheet)wb2.GetSheetAt(0);


            // Check hyperlinks again
            ClassicAssert.AreEqual(4, sheet.NumHyperlinks);
            doTestHyperlinkContents(sheet);


            // Add one more, and re-check
            IRow r17 = sheet.CreateRow(17);
            ICell r17c = r17.CreateCell(2);

            IHyperlink hyperlink = CreateHelper.CreateHyperlink(HyperlinkType.Url);
            hyperlink.Address = ("http://poi.apache.org/spreadsheet/");
            hyperlink.Label = "POI SS Link";
            r17c.Hyperlink=(hyperlink);

            ClassicAssert.AreEqual(5, sheet.NumHyperlinks);
            doTestHyperlinkContents(sheet);

            ClassicAssert.AreEqual(HyperlinkType.Url,
                    sheet.GetRow(17).GetCell(2).Hyperlink.Type);
            ClassicAssert.AreEqual("POI SS Link",
                    sheet.GetRow(17).GetCell(2).Hyperlink.Label);
            ClassicAssert.AreEqual("http://poi.apache.org/spreadsheet/",
                    sheet.GetRow(17).GetCell(2).Hyperlink.Address);


            // Save and re-load once more

            XSSFWorkbook wb3 = (XSSFWorkbook)XSSFTestDataSamples.WriteOutAndReadBack(wb2);
            ClassicAssert.AreEqual(3, wb3.NumberOfSheets);
            ClassicAssert.IsNotNull(wb3.GetSheetAt(0));
            ClassicAssert.IsNotNull(wb3.GetSheetAt(1));
            ClassicAssert.IsNotNull(wb3.GetSheetAt(2));

            sheet = (XSSFSheet)wb3.GetSheetAt(0);

            ClassicAssert.AreEqual(5, sheet.NumHyperlinks);
            doTestHyperlinkContents(sheet);

            ClassicAssert.AreEqual(HyperlinkType.Url,
                    sheet.GetRow(17).GetCell(2).Hyperlink.Type);
            ClassicAssert.AreEqual("POI SS Link",
                    sheet.GetRow(17).GetCell(2).Hyperlink.Label);
            ClassicAssert.AreEqual("http://poi.apache.org/spreadsheet/",
                    sheet.GetRow(17).GetCell(2).Hyperlink.Address);
        }

        /**
         * Only for WithMoreVariousData.xlsx !
         */
        private static void doTestHyperlinkContents(XSSFSheet sheet)
        {
            ClassicAssert.IsNotNull(sheet.GetRow(3).GetCell(2).Hyperlink);
            ClassicAssert.IsNotNull(sheet.GetRow(14).GetCell(2).Hyperlink);
            ClassicAssert.IsNotNull(sheet.GetRow(15).GetCell(2).Hyperlink);
            ClassicAssert.IsNotNull(sheet.GetRow(16).GetCell(2).Hyperlink);

            // First is a link to poi
            ClassicAssert.AreEqual(HyperlinkType.Url,
                    sheet.GetRow(3).GetCell(2).Hyperlink.Type);
            ClassicAssert.AreEqual(null,
                    sheet.GetRow(3).GetCell(2).Hyperlink.Label);
            ClassicAssert.AreEqual("http://poi.apache.org/",
                    sheet.GetRow(3).GetCell(2).Hyperlink.Address);

            // Next is an internal doc link
            ClassicAssert.AreEqual(HyperlinkType.Document,
                    sheet.GetRow(14).GetCell(2).Hyperlink.Type);
            ClassicAssert.AreEqual("Internal hyperlink to A2",
                    sheet.GetRow(14).GetCell(2).Hyperlink.Label);
            ClassicAssert.AreEqual("Sheet1!A2",
                    sheet.GetRow(14).GetCell(2).Hyperlink.Address);

            // Next is a file
            ClassicAssert.AreEqual(HyperlinkType.File,
                    sheet.GetRow(15).GetCell(2).Hyperlink.Type);
            ClassicAssert.AreEqual(null,
                    sheet.GetRow(15).GetCell(2).Hyperlink.Label);
            ClassicAssert.AreEqual("WithVariousData.xlsx",
                    sheet.GetRow(15).GetCell(2).Hyperlink.Address);

            // Last is a mailto
            ClassicAssert.AreEqual(HyperlinkType.Email,
                    sheet.GetRow(16).GetCell(2).Hyperlink.Type);
            ClassicAssert.AreEqual(null,
                    sheet.GetRow(16).GetCell(2).Hyperlink.Label);
            ClassicAssert.AreEqual("mailto:dev@poi.apache.org?subject=XSSF%20Hyperlinks",
                    sheet.GetRow(16).GetCell(2).Hyperlink.Address);
        }
        [Test]
        public void Test52716()
        {
            XSSFWorkbook wb1 = XSSFTestDataSamples.OpenSampleWorkbook("52716.xlsx");
            XSSFSheet sh1 = wb1.GetSheetAt(0) as XSSFSheet;

            XSSFWorkbook wb2 = XSSFTestDataSamples.WriteOutAndReadBack(wb1) as XSSFWorkbook;
            XSSFSheet sh2 = wb2.GetSheetAt(0) as XSSFSheet;

            ClassicAssert.AreEqual(sh1.NumberOfComments, sh2.NumberOfComments);
            XSSFHyperlink l1 = sh1.GetHyperlink(0, 1) as XSSFHyperlink;
            ClassicAssert.AreEqual(HyperlinkType.Document, l1.Type);
            ClassicAssert.AreEqual("B1", l1.CellRef);
            ClassicAssert.AreEqual("Sort on Titel", l1.Tooltip);

            XSSFHyperlink l2 = sh2.GetHyperlink(0, 1) as XSSFHyperlink;
            ClassicAssert.AreEqual(l1.Tooltip, l2.Tooltip);
            ClassicAssert.AreEqual(HyperlinkType.Document, l2.Type);
            ClassicAssert.AreEqual("B1", l2.CellRef);
        }
        [Test]
        public void Test53734()
        {
            XSSFWorkbook wb = XSSFTestDataSamples.OpenSampleWorkbook("53734.xlsx");
            XSSFHyperlink link = wb.GetSheetAt(0).GetRow(0).GetCell(0).Hyperlink as XSSFHyperlink;
            ClassicAssert.AreEqual("javascript:///", link.Address);

            wb = XSSFTestDataSamples.WriteOutAndReadBack(wb) as XSSFWorkbook;
            link = wb.GetSheetAt(0).GetRow(0).GetCell(0).Hyperlink as XSSFHyperlink;
            ClassicAssert.AreEqual("javascript:///", link.Address);
        }
        [Test]
        [Ignore("since limitation in .NET Uri class, it's impossible to accept uri like mailto:nobody@nowhere.uk%C2%A0")]
        public void Test53282()
        {
            //since limitation in .NET Uri class, it's impossible to accept uri like mailto:nobody@nowhere.uk%C2%A0
            //%C2%A0 is called non-breaking space, see https://en.wikipedia.org/wiki/Non-breaking_space
            XSSFWorkbook wb = XSSFTestDataSamples.OpenSampleWorkbook("53282.xlsx");
            XSSFHyperlink link = wb.GetSheetAt(0).GetRow(0).GetCell(14).Hyperlink as XSSFHyperlink;
            ClassicAssert.AreEqual("mailto:nobody@nowhere.uk%C2%A0", link.Address);

            wb = XSSFTestDataSamples.WriteOutAndReadBack(wb) as XSSFWorkbook;
            link = wb.GetSheetAt(0).GetRow(0).GetCell(14).Hyperlink as XSSFHyperlink;
            ClassicAssert.AreEqual("mailto:nobody@nowhere.uk%C2%A0", link.Address);
        }

        public override IHyperlink CopyHyperlink(IHyperlink link)
        {
            return new XSSFHyperlink(link);
        }

        [Test]
        public void TestCopyHSSFHyperlink()
        {
            HSSFHyperlink hlink = new HSSFHyperlink(HyperlinkType.Url);
            hlink.Address = ("http://poi.apache.org/");
            hlink.FirstColumn = (3);
            hlink.FirstRow = (2);
            hlink.LastColumn = (5);
            hlink.LastRow = (6);
            hlink.Label = ("label");
            XSSFHyperlink xlink = new XSSFHyperlink(hlink);

            ClassicAssert.AreEqual("http://poi.apache.org/", xlink.Address);
            ClassicAssert.AreEqual(new CellReference(2, 3), new CellReference(xlink.CellRef));
            // Are HSSFHyperlink.label and XSSFHyperlink.tooltip the same? If so, perhaps one of these needs renamed for a consistent Hyperlink interface
            // ClassicAssert.AreEqual("label", xlink.Tooltip);
        }


        /* bug 59775: XSSFHyperlink has wrong type if it contains a location (CTHyperlink#getLocation)
         * URLs with a hash mark (#) are still URL hyperlinks, not document links
         */
        [Test]
        public void TestURLsWithHashMark()
        {
            XSSFWorkbook wb = XSSFTestDataSamples.OpenSampleWorkbook("59775.xlsx");
            XSSFSheet sh = wb.GetSheetAt(0) as XSSFSheet;
            CellAddress A2 = new CellAddress("A2");
            CellAddress A3 = new CellAddress("A3");
            CellAddress A4 = new CellAddress("A4");
            CellAddress A7 = new CellAddress("A7");

            XSSFHyperlink link = sh.GetHyperlink(A2) as XSSFHyperlink;
            ClassicAssert.AreEqual("A2", link.CellRef, "address");
            ClassicAssert.AreEqual(HyperlinkType.Url, link.Type, "link type");
            ClassicAssert.AreEqual("http://twitter.com/#!/apacheorg", link.Address, "link target");

            link = sh.GetHyperlink(A3) as XSSFHyperlink;
            ClassicAssert.AreEqual("A3", link.CellRef, "address");
            ClassicAssert.AreEqual(HyperlinkType.Url, link.Type, "link type");
            ClassicAssert.AreEqual("http://www.bailii.org/databases.html#ie", link.Address, "link target");

            link = sh.GetHyperlink(A4) as XSSFHyperlink;
            ClassicAssert.AreEqual("A4", link.CellRef, "address");
            ClassicAssert.AreEqual(HyperlinkType.Url, link.Type, "link type");
            ClassicAssert.AreEqual("https://en.wikipedia.org/wiki/Apache_POI#See_also", link.Address, "link target");

            link = sh.GetHyperlink(A7) as XSSFHyperlink;
            ClassicAssert.AreEqual("A7", link.CellRef, "address");
            ClassicAssert.AreEqual(HyperlinkType.Document, link.Type, "link type");
            ClassicAssert.AreEqual("Sheet1", link.Address, "link target");

            wb.Close();
        }

    }
}

