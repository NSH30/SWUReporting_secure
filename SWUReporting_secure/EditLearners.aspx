<%@ Page Language="C#" Title="SWU Reporting - Edit Learners" MasterPageFile="~/Site.Master" AutoEventWireup="true"  CodeBehind="EditLearners.aspx.cs" Inherits="SWUReporting.EditLearners" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <%--<div class="form-inline">--%>
    <br />
<%--</div>--%>
    <table style="width:100%">
        <tr>
            <td  style="width:50%">
                <div class="form-inline">
                    <asp:TextBox ID="tbSearch" runat="server" CssClass="form-control"/>
                    <asp:Button ID="btnSearch" runat="server" CssClass="btn" OnClick="btnSearch_Click" Text="Search" />
                </div>
            </td>
            <td>
                <div class="form-inline">
                <asp:Button ID="btnDuplicates" runat="server" CssClass="btn" Text="Duplicate Learners" OnClick="btnDuplicates_Click" />
                </div>
            </td>
            <td>
                <div class="form-inline">                
                </div>
            </td>
        </tr>
    </table>
<div class="form-inline">
    <asp:Button ID="lnkDummy" runat="server" style="display:none" />
    <asp:Button ID="lnkDummy2" runat="server" style="display:none" />
</div>
    <br />
<cc1:ModalPopupExtender ID="ModalPopupExtender1" BehaviorID="mpe1" runat="server" PopupControlID="pnlPopup" TargetControlID="lnkDummy" BackgroundCssClass="modalBackground" CancelControlID="btnClose">
</cc1:ModalPopupExtender>
<asp:Panel ID="pnlPopup" runat="server" CssClass="modal-dialog" style="display:none" > <!-- was CssClass "panel" -->
    <div class="modal-dialog" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h3 class="modal-title">Edit Learner</h3>
            </div>
            <div class="modal-body">   
                <p><asp:Label ID="lbMessage" CssClass="alert" runat="server">Edit learner info below...</asp:Label></p>
                <br />
                <table class="table table-controls" style="vertical-align: middle">
                    <tr>
                        <td>
                            <p>Name: <asp:TextBox ID="tbName" CssClass="form-control" runat="server"></asp:TextBox></p>
                            <p>Role: <asp:TextBox ID="tbRole" CssClass="form-control" runat="server"></asp:TextBox></p>
                            <p>Country: <asp:TextBox ID="tbCountry" CssClass="form-control" runat="server"></asp:TextBox></p>
                        </td>
                        <td>
                            <p>Email: <asp:TextBox ID="tbEmail" CssClass="form-control" runat="server"></asp:TextBox></p>
                            <p>Profile: <asp:TextBox ID="tbProfile" CssClass="form-control" runat="server"></asp:TextBox></p>                            
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p>User State: <asp:TextBox ID="tbUserState" ReadOnly="true" CssClass="form-control" runat="server"></asp:TextBox></p>
                        </td>
                        <td style="vertical-align:bottom;">
                            <p><asp:CheckBox ID="cbDelete" runat="server" CssClass="form-check-input" Text="DELETE Learner" /></p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p>Company: <asp:TextBox ID="tbCompany" ReadOnly="true" CssClass="form-control" runat="server"></asp:TextBox></p>
                        </td>
                        <td>
                            <!-- company edit button -->
                            <%--<asp:Button ID="btnCompany" runat="server" Text="Edit Company" CssClass="btn" />--%>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            GEO: <asp:TextBox ID="tbGEO" ReadOnly="true" CssClass="form-control" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            <!-- GEO edit button -->
                            <%--<asp:Button ID="btnGEO" runat="server" Text="Edit GEO" CssClass="btn" />--%>
                        </td>
                    </tr>
                </table>                                                                                            
            </div>
            <div class="modal-footer">                
                <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="modal-close btn btn-primary" OnClick="btnSave_Click"/>                
                <asp:Button ID="btnClose" runat="server" Text="Close" CssClass="modal-close btn" OnClick="btnHide_Click" />
            </div>
        </div>
    </div>
</asp:Panel>

    <asp:GridView ID="gvLearners" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="false" AllowSorting="true">  <%--AllowPaging="True">--%>
        <Columns>
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:BoundField DataField="email" HeaderText="Email" />
            <asp:BoundField DataField="Role" HeaderText="Role" />
            <asp:BoundField DataField="Country" HeaderText="Country" />
            <asp:BoundField DataField="profile" HeaderText="Profile" />
            <asp:BoundField DataField="userState" HeaderText="userState" />
            <asp:BoundField DataField="VAR_Alias" HeaderText="VAR Name" />
            <asp:BoundField DataField="GEO" HeaderText="GEO" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="lnkEdit" Text="Edit" runat="server" CommandArgument='<%# Eval("ID") %>' OnClick="lnkEdit_Click"/>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="lnkActivities" Text="Activities" runat="server" CommandArgument='<%# Eval("ID") %>' OnClick="lnkActivities_Click" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
   <!-- Pop up panel to display learner activities -->
    <cc1:ModalPopupExtender ID="ModalPopupExtender2" BehaviorID="mpe2" runat="server" PopupControlID="pnlPopupAct" TargetControlID="lnkDummy2" BackgroundCssClass="modalBackground" CancelControlID="btnCloseAct"/>
    <asp:Panel ID="pnlPopupAct" runat="server" CssClass="modal-dialog" style="width:1250px; display:none;"> <!-- was CssClass "panel" -->
    <div class="modal-dialog" style="width:1250px; max-height:800px; overflow: auto;" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h3 class="modal-title">Learner Activities</h3>
            </div>
            <div class="modal-body">   
                <%--<p><asp:Label ID="Label1" CssClass="alert" runat="server">Summary of learner activities...</asp:Label></p>
                <br />--%>
                <table class="table table-controls" style="vertical-align: middle">
                    <tr>
                        <td>
                            <p>Name: <asp:TextBox ID="tbNameAct" CssClass="form-control" ReadOnly="true" runat="server"></asp:TextBox></p>
                            <p>Company: <asp:TextBox ID="tbCompanyAct" ReadOnly="true" CssClass="form-control" runat="server"></asp:TextBox></p>
                        </td>
                        <td>
                            <p>Email: <asp:TextBox ID="tbEmailAct" CssClass="form-control" ReadOnly="true" runat="server"></asp:TextBox></p>
                            
                        </td>
                    </tr>
                </table>                                                                                            
<%--            </div>
            <div class="modal-body">--%>
                <asp:GridView ID="gvActivities" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="false" AllowSorting="true" AllowPaging="False">
                    <Columns>
                    <asp:BoundField DataField="CourseName" HeaderText="Course Name" />
                    <asp:BoundField DataField="CourseAlias" HeaderText="Course Alias" />
                    <asp:BoundField DataField="Type" HeaderText="Course Type" />
                    <asp:BoundField DataField="status" HeaderText="Status" />
                    <asp:BoundField DataField="startDate" HeaderText="Start Date" />
                    <asp:BoundField DataField="completionDate" HeaderText="Completion Date" />                    
                    </Columns>
                </asp:GridView>
            </div>
            <div class="modal-footer">                
                <asp:Button ID="btnCloseAct" runat="server" Text="Close" CssClass="modal-close btn" OnClick="btnHide_Click" />
            </div>
        </div>
    </div>
</asp:Panel>  
<h4><asp:Label ID="lblMessage" Text="" runat="server"></asp:Label></h4>
</asp:Content>
