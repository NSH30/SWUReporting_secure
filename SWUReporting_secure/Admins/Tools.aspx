<%@ Page Language="C#" Title="SWU Reporting - Tools" MasterPageFile="/Site.Master"  AutoEventWireup="true" CodeBehind="Tools.aspx.cs" Inherits="SWUReporting.Tools" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Admin Tools</h3>
    <div class="form-inline">
        <asp:Button ID="lnkDummy" runat="server" style="display:none" />
    </div>
        <div class="container p-5 shadow" style="background-color:whitesmoke">
        <div>
            <h4>Batch Delete Learners</h4>
            <p>Mark multiple learners as DELETED by email address</p>
        </div>
        <div >
            <br />
            <asp:Button ID="btnBatchDelete" Text="Batch Delete" CssClass="btn btn-primary" runat="server" OnClick="btnBatchDelete_Click"/>
        </div>  
        <br />
        <asp:Label ID="lblBDResponse" Text ="" CssClass="label label-info" runat="server"></asp:Label>      
    </div>
    <br/>
    <br/>

    <div class="form-inline">
        <asp:Button ID="addcourse" runat="server" style="display:none" />
    </div>
        <div class="container p-5 shadow" style="background-color:whitesmoke">
        <div>
            <h4>Add Course Completions</h4>
            <p>Manually add courses and course completions directly to the reporting database</p>
        </div>
        <div >
            <br />
            <asp:Button ID="btnAddCourse" Text="Add" CssClass="btn btn-primary" runat="server" OnClick="btnAddCourse_Click"/>
        </div>  
        <br />
        <div class="card">
            <asp:TextBox ID="tbMessage" runat="server" BorderStyle="None" TextMode="MultiLine" Rows="10" Visible="False" Width="100%"></asp:TextBox>
        </div>     
    </div>
    <br/>
    <br/>

    <!--Add/Edit Users -->
    <div class="form-inline">
        <asp:Button ID="AddUsers" runat="server" style="display:none" />
    </div>
        <div class="container p-5 shadow" style="background-color:whitesmoke">
        <div>
            <h4>Add/Edit Users</h4>
            <p>Manually add or Edit user access for admin or non-admin privileges</p>
        </div>
        <div >
            <br />            
                    <asp:TextBox ID="UserTextbox" runat="server" CssClass="form-control" placeholder="Search" Style="display:inline"></asp:TextBox>             
                    <asp:LinkButton runat="server" ID="btnUserSearch" CssClass="btn form-inline" placeholder="Search" OnClick="btnUserSearch_Click">                    
                    <span aria-hidden="true" class="btn glyphicon glyphicon-search" />
                    </asp:LinkButton>                                    
        </div>  
        <br />         
    </div>
<%--    <div class="container p-5 shadow" style="background-color:whitesmoke">
        <div>
            <h4>Download Raw Learner Data</h4>
            <div class="form-inline">
                <asp:TextBox ID="tbSearchFilter" placeholder="Search filter" runat="server"></asp:TextBox>
                <asp:DropDownList ID="ddGEOs" CssClass="form-control" runat="server" style="width:auto" AutoPostBack="true"></asp:DropDownList>
                <asp:CheckBox ID="cbShowDeleted" CssClass="checkbox" runat="server" Text="Include DELETED users" />
            </div>
        <div >
            <br />
            <asp:Button ID="btnDownload" Text="Download" CssClass="btn btn-primary" runat="server" OnClick="btnDownload_Click"/>
            </div>  
            <br />
        </div>
    </div>--%>

    <!-- Pop up panel to display learner activities -->
    <cc1:ModalPopupExtender ID="ModalPopupExtender2" BehaviorID="mpe1" runat="server" PopupControlID="Panel2" TargetControlID="addcourse" BackgroundCssClass="modalBackground" CancelControlID="btnCloseCourse" />
    <asp:Panel ID="Panel2" runat="server" CssClass="modal-dialog" Style="display: none;">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h3 class="modal-title">Add Course Completions</h3>
                </div>
                <div class="modal-body">
                    <p>Course Name: &nbsp &nbsp &nbsp <asp:TextBox ID="tbName" CssClass="form-control " Width="250px" runat="server" Rows="1"></asp:TextBox></p>
                    <p>Completion Date: &nbsp <asp:TextBox ID="tbDate" CssClass="form-control" Width="250px" runat="server" type="date" MaxLength="9" Rows="1"></asp:TextBox></p>
                    <p>Learner emails:<asp:TextBox ID="tbEmailsCheck" CssClass="form-control" runat="server" Rows="5" TextMode="MultiLine"></asp:TextBox>
                    </p>
                    <asp:Button ID="btnCheck" runat="server" Text="Check Email" CssClass="modal-close btn" OnClick="btnCheck_Click" Visible="False" />
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnUpdateCourse" runat="server" Text="Update" CssClass="btn btn-primary" OnClick="btnUpdateCourse_Click" />
                    <asp:Button ID="btnCloseCourse" runat="server" Text="Close" CssClass="modal-close btn" OnClick="btnCloseCourse_Click" />                    
                </div>                
            </div>
        </div>
    </asp:Panel>

    <!--Add/Edit Users -->
<%--    <cc1:ModalPopupExtender ID="ModalPopupExtender3" BehaviorID="mpe3" runat="server" PopupControlID="PanelUsers" TargetControlID="AddUsers" BackgroundCssClass="modalBackground" CancelControlID="btnCloseUsers" />
    <asp:Panel ID="PanelUsers" runat="server" CssClass="modal-dialog" Style="display: none;">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h3 class="modal-title">Add/Edit Users</h3>
                </div>
                <div class="modal-body">
                    <p>First Name: &nbsp &nbsp &nbsp <asp:TextBox ID="UserFname" CssClass="form-control " Width="250px" runat="server" Rows="1"></asp:TextBox></p>
                    <p>Last Name: &nbsp &nbsp &nbsp <asp:TextBox ID="UserLname" CssClass="form-control " Width="250px" runat="server" Rows="1"></asp:TextBox></p>
                    <p>Trigram: &nbsp &nbsp &nbsp <asp:TextBox ID="UserTrigram" CssClass="form-control " Width="250px" runat="server" Rows="1"></asp:TextBox></p>
                  <div>  
                    <asp:RadioButton ID="Admin" runat="server" Text="Yes" GroupName="Admin" />  
                    <asp:RadioButton ID="NotAdmin" runat="server" Text="No" GroupName="Admin" />  
                  </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnAddUsers" runat="server" Text="Add Users" CssClass="modal-close btn" Onclick="btnAddUsers_click"/>
                    <asp:Button ID="btnCloseUsers" runat="server" Text="Close" CssClass="modal-close btn" OnClick="btnCloseUsers_Click"/>
                   <asp:Button ID="btnEditUsers" runat="server" Text="Edit Users" CssClass="modal-close btn" Onclick="btnEditUsers_click"/>
               </div>                
            </div>
        </div>
    </asp:Panel>--%>

    <!--Delete Learners --->
    <cc1:ModalPopupExtender ID="ModalPopupExtender1" BehaviorID="mpe2" runat="server" PopupControlID="pnlEmails" TargetControlID="lnkDummy" BackgroundCssClass="modalBackground" CancelControlID="btnClose"/>
    <asp:Panel ID="pnlEmails" runat="server" CssClass="modal-dialog" style="display:none;"> <!-- was CssClass "panel" -->
    <div class="modal-dialog" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h3 class="modal-title">Batch Delete Learners</h3>
            </div>
            <div class="modal-body">  
                <p>Learner emails:<asp:TextBox ID="tbEmails" CssClass="form-control" runat="server" Rows="5" TextMode="MultiLine"></asp:TextBox> </p>
            </div>
            <div class="modal-footer">                
                <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="btn btn-primary" OnClick="btnUpdate_Click" />
                <asp:Button ID="btnClose" runat="server" Text="Close" CssClass="modal-close btn" OnClick="btnClose_Click" />
            </div>
        </div>
    </div>
</asp:Panel>  
</asp:Content>
