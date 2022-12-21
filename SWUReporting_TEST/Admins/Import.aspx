<%@ Page Language="C#" Title="SWU Reporting - Import" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" MasterPageFile="~/Site.Master"  CodeBehind="Import.aspx.cs" Inherits="SWUReporting.Import" %>

<%@ Register Src="~/Controls/EditLearnersControl.ascx" TagPrefix="uc1" TagName="ELC" %>

<%@ Register Src="~/Controls/EditCompanyControl.ascx" TagPrefix="uc2" TagName="ECC" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- import Learner Transcripts -->
    <div class="container p-5 shadow" style="background-color:whitesmoke; margin-bottom:10px;">
        <h3>Import learner transcripts from SOLIDWORKS University</h3>
        <div class="container">
            <div class="row">
                <div class="col-md-4" style="width:auto">
                    <ol>
                        <li>Download a full learner transcript for All Learners since the last update.</li>
                        <li>Extract the learner transcript csv file from the zip archive.</li>
                        <li>Upload the csv file here.</li>
                        <li>Importing may take several minutes to complete.  </li>
                        <li>NEW Oct. 14, 2021: All Unenrolled activities will be automatically removed.</li>
                    </ol>
                </div>
            </div>
        </div>
        <div class="card">
            <div class="card-body">
                <h4 class="card-title">
                    Learner Transcript upload
                </h4>            
                    <asp:FileUpload ID="Uploader" runat="server" ToolTip="Browse to a Learner Transcript csv file to upload..." CssClass="btn"/>
                    <br />
                    <asp:Button ID="btnUpload" runat="server" OnClick="btnUpload_Click" Text="Import LT" CssClass="btn btn-primary" />
                    <br />
                    <br />
                    <asp:Label ID="lblMessage" runat="server"/>
            </div>
        </div>
        <div class="card">
            <asp:TextBox ID="tbMessage" runat="server" BorderStyle="None" TextMode="MultiLine" Rows="10" Visible="False" Width="100%" OnTextChanged="tbMessage_TextChanged"></asp:TextBox>
        </div>
    </div>
    <br />
    <%-- Review buttons and datagrids --%>
        <asp:Panel CssClass="panel" ID="pnlReview" runat="server" Visible="False">
        <table class="table table-responsive" style="width: 50%;">
            <tr>
                <td>
                    <asp:Button  class="btn" ID="btnReviewLearners" runat="server" Text="Review Learners" OnClick="btnReviewLearners_Click" />
                </td>
                <td>
                    <asp:Button  class="btn" ID="btnReviewCompanies" runat="server" Text="Review Companies" OnClick="btnReviewCompanies_Click" Visible="True" />
                </td>
                <td>
                    <asp:Button  class="btn" ID="btnReviewCourses" runat="server" Text="Review Courses" OnClick="btnReviewCourses_Click" Visible="False" />
                </td>
            </tr>
        </table>
    </asp:Panel>

    <!-- import FTE data -->
    <br />
    <div class="container p-5 shadow" style="background-color:#CCFFCC; margin-bottom:10px;">
        <h3>Import FTE data</h3>
        <div class="container">
            <div class="row">
                <div class="col-md-4" style="width:auto">
                    <ol>
                        <li>Check FTE Excel workbook for errors.&nbsp; ie: Correct users with SolidWorks as the organization without a DS email address.</li>
                        <li>Remove all columns after GEO</li>
                        <li>Check for duplicate email addresses</li>
                        <li>Verify Column names: [Employment Status] [Individual Quota?] [FTE Role] [Secondary FTE Role] [FTE Status] 
                            <br/>[FTE Value] [Login Name] [First Name] [Last Name] [Contact Id] [Account] [Account Status] [Account Type] [Organization] [Email] [Country] [Geo]</li>
                        <li>Save the worksheet as UTF-8 Unicode tab-delimited text.</li>
                        <li>Browse to the saved file below.</li>
                        <li>Click Import FTE.</li>
                    </ol>
                </div>
            </div>
        </div>
        <div class="card">
            <div class="card-body">
                        <h4 class="card-title">FTE report upload </h4>
                        <asp:FileUpload ID="UploaderFTE" runat="server" ToolTip="Browse to the FTE file..." CssClass="btn"/>
                        <br />
                        <asp:Button ID="btnUploadFTE" runat="server" OnClick="btnUploadFTE_Click" Text="Import FTE" CssClass="btn btn-danger" />
                        <br />
                        <br />
                        <asp:Label ID="lblMessageFTE" runat="server"/>
                    </div>
            <asp:TextBox ID="tbMessageFTE" runat="server" BorderStyle="None" TextMode="MultiLine" Rows="10" Visible="False" Width="100%"></asp:TextBox>
        </div>
    </div>

    <!-- Import Virtual Tester Data -->
    <br />
    <div class="container p-5 shadow" style="background-color:#b3c6ff; margin-bottom:10px;">
        <h3>Import Virtual Tester data</h3>
        <div class="container">
            <div class="row">
                <div class="col-md-4" style="width:auto">
                    <ol>
                        <li>Check Virtual Tester CSV for errors.</li>
                        <li>CSV column names should match: [Firstname],[Lastname],[Name],[email],[Adobe Captivate id],[Certificate name],[Certificate full name],[Completion Date (UTC TimeZone)],[Status],[Employee Status],[Company],[User GEO],[Country]</li>                        
                        <li>Open CSV file with Notepad++, Edit, EOL Conversion, Windows (CR LF), Save, close file.</li>
                        <li>Browse to the saved file below.</li>
                        <li>Click Import VT.</li>
                    </ol>
                </div>
            </div>
        </div>       
        <div class="card">
            <div class="card-body">
                <h4 class="card-title">
                    Virtual Tester report upload
                </h4>            
                    <asp:FileUpload ID="UploadVT" runat="server" ToolTip="Browse to the Virtual Tester data..." CssClass="btn"/>
                    <br />
                    <asp:Button ID="btnImportVT" runat="server" OnClick="btnImportVT_Click" Text="Import VT" CssClass="btn btn-warning" />
                    <br />
                    <br />
                    <asp:Label ID="lblMessageVT" runat="server"/>
                <br />
                <asp:Button Enabled="false" ID="btnDownloadVTRes" runat="server" OnClick="btnDownloadVTRes_Click" Text="Download Imported Learners" 
                    CssClass="btn" ToolTip="Download a list of learners who were added. This may include duplicates." />
                <asp:Button Enabled="false" ID="btnDownloadVTCourses" runat="server" OnClick="btnDownloadVTCourses_Click" Text="Download Imported Courses"
                    CssClass="btn" ToolTip="Download a list of courses that were added. These may need to be mapped to a course alias." />
            </div>
        </div>
        <div class="card">
            <asp:TextBox ID="tbMessageVT" runat="server" BorderStyle="None" TextMode="MultiLine" Rows="10" Visible="False" Width="100%"></asp:TextBox>
        </div>
    </div>

    <uc1:ELC runat="server" ID="elc1" Visible="False" />
    <uc2:ECC runat="server" ID="ecc1" Visible="False" />

</asp:Content>

