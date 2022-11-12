<%@ Page Language="C#" Title="SWU Reporting - Home" MaintainScrollPositionOnPostback="true"  AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="Default.aspx.cs" Inherits="SWUReporting._Default" %>
<%@ Register Src="~/Controls/ViewActivitiesControl.ascx" TagPrefix="uc9" TagName="ViewActivitiesControl" %>
<%@ Register Src="~/Controls/Loading.ascx" TagPrefix="ucLoading" TagName="LoadingControl" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">  
    <asp:UpdatePanel ID="up2" runat="server" >
        <ContentTemplate>
    <asp:Button ID="btnDebugTest" runat="server" CssClass="btn btn-danger" Text="Testing" OnClick="btnDebugTest_Click" Visible ="false"  />
            </ContentTemplate>
        </asp:UpdatePanel>
<%--        <asp:UpdateProgress ID="UpdateProgress2" runat="server" AssociatedUpdatePanelID="up2">
        <ProgressTemplate>
            <ucLoading:LoadingControl ID="ucl2" runat="server" />
        </ProgressTemplate>
    </asp:UpdateProgress>  --%>  
    <h3>Download dashboard reports</h3>
    <p>Select the desired report to download.</p>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
    <div id="GEOrpt" class="container p-5 shadow" style="background-color:whitesmoke; margin-bottom:10px;">
        <h4>GEO Dashboard Reports</h4>
        <p>Standard GEO reports for the world wide dashboard, including the complete transposed report</p>
        <p><asp:Button ID="btnGEOReport" Text="GEO Report" CssClass="btn btn-primary" runat="server" OnClick="btnGEOReport_Click" />            
        </p>
    </div>
        </ContentTemplate>
        <Triggers><asp:PostBackTrigger ControlID="btnGEOReport" /></Triggers>
    </asp:UpdatePanel>
    <br />
    <asp:UpdatePanel ID="upVAR" runat="server">
        <ContentTemplate>
            <div class="container p-5 shadow" style="background-color:whitesmoke; margin-bottom:10px;">
                <div>
                    <h4>VAR Dashboard Reports</h4>
                    <p>Select the GEO first, then select the VAR names to export.</p>
                    <p><a href="EditCompanies.aspx">Click here</a> to modify or add VAR names to each GEO.</p>
                </div>
                    <%--<br />--%>
                    <p><asp:Button ID="btnDownloadAllVAR" Text="Download All" CssClass="btn btn-danger" runat="server" OnClick="btnDownloadAllVAR_Click" ToolTip="Download all VAR dashboard files - takes several minutes" /></p>
            
                <div >
                    GEO Filter:
                    <br />
                    <asp:DropDownList ID="ddGEOsVAR" CssClass="form-control" runat="server" style="width:auto" OnSelectedIndexChanged="ddGEOsVAR_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>            
                    <table class="table" id="tblUnselect" visible="false">
                        <tr>
                            <td>
                                <asp:CheckBox Checked="false" ID="cbSelectAll" Text=" Select All" Font-Bold="true" CssClass="checkbox form-check-label" runat="server" OnCheckedChanged="cbSelectAll_CheckedChanged" AutoPostBack="true"/>
                            </td>                    
                        </tr>
                    </table>

                    <div style="margin:20px">
                    <asp:CheckBoxList ID="cbListVARs" runat="server" CssClass="checkbox form-check-label" Visible="False">
                    </asp:CheckBoxList>
                    </div>
                    <br />
                    <p><asp:Button ID="btnVARReport" Text="Download" CssClass="btn btn-primary" runat="server" OnClick="btnVARReport_Click" OnClientClick="javascript:showWait();" /></p>
            
                </div>        
            </div>
        </ContentTemplate>   
        <Triggers>
            <asp:PostBackTrigger ControlID="btnDownloadAllVAR" />
            <asp:PostBackTrigger ControlID="btnVARReport" />
            <%--<asp:AsyncPostBackTrigger ControlID="ddGEOsVAR" />--%>
        </Triggers>     
    </asp:UpdatePanel>
    <asp:updateprogress associatedupdatepanelid="upVAR"
        id="updateProgress" runat="server">
        <progresstemplate>
            <div id="progressBackgroundFilter"></div>
            <div id="processMessage"><br /><br />
                 <img alt="Loading" src="Images/Fadingballs.gif" />
            </div>
        </progresstemplate>
    </asp:updateprogress>
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
        <ProgressTemplate>
            <ucLoading:LoadingControl ID="ucl1" runat="server" />
        </ProgressTemplate>
    </asp:UpdateProgress>
    <br />
    <div class="container p-5 shadow" style="background-color:whitesmoke; margin-bottom:10px;">
        <div>
            <h4>GEO Transposed Report</h4>
            <p>Standard CSV transposed report that can be filtered by GEO</p>
        </div>
        <div >
            GEO Filter:
            <br />
            <asp:DropDownList ID="ddGEOs" CssClass="form-control" runat="server" style="width:auto"></asp:DropDownList>
            <br />
            <p><asp:Button ID="btnGEOTrans" Text="Transposed Report" CssClass="btn btn-primary" runat="server" OnClick="btnGEOTrans_Click" /></p>

        </div>        
    </div>
    <br />
    <%-- View and Download Learner Activities --%>
    <div class="container p-5 shadow" style="background-color:whitesmoke">
        <div>
            <h4>Learner Activities</h4>                    
            <asp:Panel CssClass="form-inline" ID="LearnerActivities" runat="server" DefaultButton="lnkSearch">
                <asp:Table runat="server" style="width:100%">
                    <asp:TableRow style="">
                        <asp:TableCell  CssClass="form-inline" style="padding-right:10px">
                            Starting date: <asp:TextBox ID="tbSearchDate" TextMode="Date" runat="server" CssClass="form-inline form-control"></asp:TextBox>
                        </asp:TableCell>
                        <asp:TableCell CssClass="form-inline">
                            Ending date: <asp:TextBox ID="tbSearchEndDate" TextMode="Date" runat="server" CssClass="form-inline form-control"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>      
                <br/>
                <asp:TextBox ID="tbSearchString" runat="server" CssClass="form-inline form-control" placeholder="Search" ToolTip="Enter course name, learner name, email, company or country..."></asp:TextBox>
                <%--<asp:Button ID="btnSearch" runat="server" CssClass="btn btn-outline-primary form-inline" OnClick="btnSearch_Click" Text="Search" />--%>
                <asp:LinkButton runat="server" ID="lnkSearch" CssClass="btn form-inline" placeholder="Search" OnClick="btnSearch_Click">
                    <span aria-hidden="true" class="btn glyphicon glyphicon-search" />
                </asp:LinkButton>
                <asp:CheckBox CssClass="checkbox form-inline" ID="cbCompleted" runat="server" Text=" Completed Only" Checked="True"/>
                <asp:CheckBox CssClass="checkbox form-inline" ID="cbSalesOnly" runat="server" Text=" Sales Roles Only" />
            </asp:Panel>
        </div>  
        <br />
        <uc9:ViewActivitiesControl runat="server" id="vac" Visible="false"/>
     </div>
    
    <br/>
    <%-- Download raw learner data report --%>
    <div class="container p-5 shadow" style="background-color:whitesmoke" >
        <div>
            <h4>Download Raw Learner Data</h4>
            <div class="form-inline">
                <asp:TextBox CssClass="form-inline form-control" ID="tbSearchFilter" placeholder="Search filter" runat="server" ToolTip="Enter course name, learner name, company, course or leave empty for all entries."></asp:TextBox>
                <asp:DropDownList ID="ddGEOs2" CssClass="form-control form-inline" runat="server" style="width:auto" AutoPostBack="true"></asp:DropDownList>
                <asp:CheckBox ID="cbShowDeleted" CssClass="checkbox form-inline" runat="server" Text=" Include DELETED users" />
                <asp:CheckBox Checked="false" ID="cbCompletedOnly" Text=" Completed Only" CssClass="checkbox form-inline" runat="server" ToolTip="Include results for completed courses only."/>
            </div>
        <div >
            <br />
            <asp:Button ID="btnDownload" Text="Download" CssClass="btn btn-primary" runat="server" OnClick="btnDownload_Click"/>
            </div>  
            <br />
        </div>
    </div>
    <br />
    <%-- special learner report download: transposed report with points instead of dates --%>
    <asp:Panel ID="userPoints" runat="server">
        <div  class="container p-5 shadow" style="background-color:whitesmoke">
            <div>
                <h4>Download Learner Progress Report</h4>
                <p>This report shows alignment points instead of completion dates and can be used to check user scores</p>
                <asp:DropDownList ID="ddGEOs3" CssClass="form-control form-inline" runat="server" style="width:auto" AutoPostBack="true"></asp:DropDownList>
                <br /><p><asp:Button ID="btnLearnerProgress" Text="Learner Progress Report" CssClass="btn btn-primary" runat="server" OnClick="btnLearnerProgress_Click" />            
                </p>
            </div>
        </div>
    </asp:Panel>
 <script type="text/javascript">
     <%--var prm = Sys.WebForms.PageRequestManager.getInstance();
     prm.add_endRequest(EndRequest);
     prm.add_initializeRequest(InitializeRequest);
     var postBackElement;
     function InitializeRequest(sender, args) {         
         postBackElement = args.get_postBackElement();         
         alert('Initializing:' + postBackElement.id);
         if (postBackElement.id == 'btnVARReport') {
             $get('<%= updateProgress.ClientID %>').style.display = 'block';
         }
     }

     function EndRequest(sender, args) {
         alert('Ending');
         alert('Ending:' + postBackElement.id);
         if (postBackElement.id == 'btnVARReport') {
             $get('<%= updateProgress.ClientID %>').style.display = 'none';
         }
     }

     function showWait() {
         //do nothing here
         <%--$get('<%= updateProgress.ClientID %>').style.display = 'block';

     }--%>
 </script>
</asp:Content>
