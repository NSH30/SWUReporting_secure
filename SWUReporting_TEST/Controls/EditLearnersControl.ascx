<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditLearnersControl.ascx.cs" Inherits="SWUReporting.EditLearnersControl" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<!-- hidden dummy controls for the modalpopupextender panels -->
<div class="form-inline">
    <asp:Button ID="lnkDummy" runat="server" style="display:none" />
    <asp:Button ID="lnkDummy2" runat="server" style="display:none" />
    <asp:Button ID="lnkDummy3" runat="server" style="display:none" />
    <asp:Button ID="lnkDummy4" runat="server" style="display:none" />
    <asp:Button ID="lnkDummy5" runat="server" style="display:none" />
</div>
<br />
<%--<cc1:ModalPopupExtender ID="ModalPopupExtender1" BehaviorID="mpe1" runat="server" PopupControlID="pnlPopup" TargetControlID="lnkDummy" BackgroundCssClass="modalBackground" CancelControlID="btnClose">--%>
<cc1:ModalPopupExtender ID="ModalPopupExtender1" BehaviorID="mpe1" runat="server" PopupControlID="pnlPopup" TargetControlID="lnkDummy" BackgroundCssClass="modalBackground" CancelControlID="btnClose">
</cc1:ModalPopupExtender>
<asp:Panel ID="pnlPopup" runat="server" CssClass="modal-dialog-centered modal-xl" style="display:block;" > <!-- was CssClass "panel" -->
        <div class="modal-content">
            <div class="modal-header">
                <h3>Edit Learner</h3>
            </div>
            <div class="modal-body" style="max-height: 500px; overflow: auto;">   
                *Please notify the <a href="mailto:solidworks.salescertification@3ds.com">SOLIDWORKS University team</a> about any changes to learner details so the changes can also be made in SOLIDWORKS University.<br /> &nbsp;<table class="table table-controls" style="vertical-align: middle; width: 100%;">
                    <tr>
                        <td>
                            <p>
                                Name:
                                <asp:TextBox ID="tbName" runat="server" CssClass="form-control"></asp:TextBox>
                            </p>
                            <p>
                                Role:
                                <asp:TextBox ID="tbRole" runat="server" CssClass="form-control"></asp:TextBox>
                            </p>
                            <p>
                                Country:
                                <asp:TextBox ID="tbCountry" runat="server" CssClass="form-control"></asp:TextBox>
                            </p>
                        </td>
                        <td>
                            <p>
                                Email:
                                <asp:TextBox ID="tbEmail" runat="server" CssClass="form-control"></asp:TextBox>
                            </p>
                            <p>
                                Profile:
                                <asp:TextBox ID="tbProfile" runat="server" CssClass="form-control"></asp:TextBox>
                            </p>
                            <p>
                                FTE Value:
                                <asp:TextBox ID="tbFTEVal" runat="server" CssClass="form-control" ToolTip="This value will import from the FTE report monthly."></asp:TextBox>
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p>
                                User State:
                                <asp:TextBox ID="tbUserState" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                            </p>
                        </td>
                        <td style="vertical-align:bottom;">
                            <p>
                                <asp:CheckBox ID="cbDelete" runat="server" CssClass="form-check-input" Text="DELETE Learner" />
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p>
                                Company:
                                <asp:TextBox ID="tbCompany" runat="server" CssClass="form-control" ReadOnly="false"></asp:TextBox>
                            </p>
                        </td>
                        <td>
                            <!-- company edit button -->
                            <p>
                                VAR Alias:
                                <asp:DropDownList Visible="false" ID="ddVARAlias" runat="server" CssClass="form-control" OnSelectedIndexChanged="ddVARAlias_SelectedIndexChanged">
                                </asp:DropDownList>
                                <asp:TextBox ID="tbVARAlias" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>GEO:
                            <asp:TextBox ID="tbGEO" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                        </td>
                        <td>
                            <!-- GEO edit button --><%--<asp:Button ID="btnGEO" runat="server" Text="Edit GEO" CssClass="btn" />--%>
                            <p>
                                Update GEO:<asp:DropDownList ID="ddGEOs" runat="server" CssClass="form-control" style="width:auto" />
                            </p>
                        </td>
                    </tr>
                </table>
            </div>
            <div class="modal-footer">                
                <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="modal-close btn btn-primary" OnClick="btnSave_Click"/>                
                <asp:Button ID="btnClose" runat="server" Text="Close" CssClass="modal-close btn" OnClick="btnHide_Click" />
            </div>
        </div>
</asp:Panel>

<div class="form-inline">
    <asp:Button ID="btnMergeLearners" style="margin-bottom:10px" Text="Merge Learners" CssClass="btn btn-primary tooltips" runat="server" Enabled="false" OnClick="btnMergeLearners_Click" data-placement="right" title="Before merging two users, edit them to match all field values." />
    <asp:LinkButton ID="lnkSaveExcel" CssClass="btn btn-primary" runat="server" OnClick="lnkSaveExcel_Click" style="margin-bottom:10px" ToolTip="Download">
        <span aria-hidden="true" class="glyphicon glyphicon-download-alt"></span>
    </asp:LinkButton>
</div>
<br />
    <asp:GridView ID="gvLearners" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="false" AllowSorting="true" OnRowDataBound="gvLearners_RowDataBound">  <%--AllowPaging="True">--%>
        <Columns>
            <asp:TemplateField HeaderText="Merge">
                <ItemTemplate>
                    <asp:CheckBox ID="chkMerge" runat="server" CommandArgument='<%# Eval("ID") %>' OnCheckedChanged="chkMerge_CheckedChanged" AutoPostBack="true" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:BoundField DataField="email" HeaderText="Email" />
            <asp:BoundField DataField="Role" HeaderText="Role" />
            <asp:BoundField DataField="Country" HeaderText="Country" />
            <asp:BoundField DataField="profile" HeaderText="Profile" />
            <asp:BoundField DataField="userState" HeaderText="userState" />
            <asp:BoundField DataField="VAR_Alias" HeaderText="VAR Name" />
            <asp:BoundField DataField="GEO" HeaderText="GEO" />
            <asp:BoundField DataField="fte_value" HeaderText="FTE Value" />
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

  <!-- pop up panel to show duplicate user list with download option -->
    <cc1:ModalPopupExtender ID="ModalPopupExtender3" BehaviorID="mpe3" runat="server" PopupControlID="pnlPopupDup" TargetControlID="lnkDummy3" BackgroundCssClass="modalBackground" CancelControlID="btnCloseDup" />
    <asp:Panel ID="pnlPopupDup" runat="server" CssClass="modal-dialog-centered modal-xl" style="display:block;">
        <div class="modal-content">
            <div class="modal-header">
                <h3 class ="modal-title">Duplicate Learners</h3>
            </div>
            <div class="modal-body" style="max-height: 500px; overflow: auto;">
                <div class="form-inline">                    
                    <asp:LinkButton ID="lnkDownloadDup" CssClass="btn btn-primary" runat="server" OnClick="lnkDownloadDup_Click" style="margin-bottom:10px" ToolTip="Download">
                        <span aria-hidden="true" class="glyphicon glyphicon-download-alt"></span>
                    </asp:LinkButton>
                </div>
                <br />
                <asp:GridView ID="gvDuplicates" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="false" AllowSorting="true" AllowPaging="False">
                    <Columns>
                    <asp:BoundField DataField="count" HeaderText="Duplicate Count" />
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:BoundField DataField="VAR" HeaderText="VAR" />
                    <asp:BoundField DataField="Country" HeaderText="Country" />             
                    </Columns>
                </asp:GridView>
            </div>
            <div class="modal-footer">
                <asp:Button ID="btnCloseDup" runat="server" Text="Close" CssClass="modal-close btn" OnClick="btnCloseDup_Click" />
            </div>
        </div>
    </asp:Panel>
   <!-- Pop up panel to display learner activities -->
    <cc1:ModalPopupExtender ID="ModalPopupExtender2" BehaviorID="mpe2" runat="server" PopupControlID="pnlPopupAct" TargetControlID="lnkDummy2" BackgroundCssClass="modalBackground" CancelControlID="btnCloseAct"/>
    <asp:Panel ID="pnlPopupAct" runat="server" CssClass="modal-dialog-centered modal-xl" style="display:block;"> <!-- was CssClass "panel" -->
    
        <div class="modal-content">
            <div class="modal-header">
                <h3 class="modal-title">Learner Activities</h3>
            </div>
            <div class="modal-body" style="max-height: 500px; overflow: auto;">   
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

</asp:Panel>  
<!-- pop up panel to show users missing data with download option -->
    <cc1:ModalPopupExtender ID="ModalPopupExtender4" BehaviorID="mpe4" runat="server" PopupControlID="pnlPopupMissing" TargetControlID="lnkDummy4" BackgroundCssClass="modalBackground" CancelControlID="btnCloseMissing" />
    <asp:Panel ID="pnlPopupMissing" runat="server" CssClass="modal-dialog-centered modal-xl" style="display:block;">
        <div class="modal-content">
            <div class="modal-header">
                <h3 class ="modal-title">Learners Missing Data</h3>
            </div>
            <div class="modal-body" style="max-height: 500px; overflow: auto;">
                <div class="form-inline">                    
                    <asp:LinkButton ID="btnDownloadMissing" CssClass="btn btn-primary" runat="server" OnClick="btnDownloadMissing_Click" style="margin-bottom:10px" ToolTip="Download">
                        <span aria-hidden="true" class="glyphicon glyphicon-download-alt"></span>
                    </asp:LinkButton>
                </div>
                <br />
                <asp:GridView ID="gvMissing" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="false" AllowSorting="true" AllowPaging="False">
                    <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:BoundField DataField="Email" HeaderText="email" />
                    <asp:BoundField DataField="Role" HeaderText="Role" />
                    <asp:BoundField DataField="Country" HeaderText="Country" />
                    <asp:BoundField DataField="VAR" HeaderText="VAR" />
                    <asp:BoundField DataField="GEO" HeaderText="GEO" />      
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkEditBlanks" Text="Edit" runat="server" CommandArgument='<%# Eval("ID") %>' OnClick="lnkEditBlanks_Click"/>
                        </ItemTemplate>
                    </asp:TemplateField>       
                    </Columns>
                </asp:GridView>
            </div>
            <div class="modal-footer">
                <asp:Button ID="btnCloseMissing" runat="server" Text="Close" CssClass="modal-close btn" OnClick="btnCloseMissing_Click" />
            </div>
        </div>
    </asp:Panel>

<%-- pop up panel to confirm merging the right users --%>
<cc1:ModalPopupExtender ID="mpeConfirm" BehaviorID="mpeC" runat="server" PopupControlID="pnlConfirmMerge" TargetControlID="lnkDummy5" BackgroundCssClass="modalBackground" CancelControlID="btnCancelMerge" />
<asp:Panel ID="pnlConfirmMerge" runat="server" CssClass="modal-dialog-centered modal-sm" style="display:block;">
    <div class="modal-content">
        <div class="modal-header">
            <h3 class ="modal-title">Confirm Merge Learners</h3>
        </div>
        <div class="modal-body" style="max-height: 300px; overflow: auto;">
            <asp:Label runat="server">Continue to merge these users?</asp:Label>
            <asp:GridView ID="gvMergeLearners" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="false" AllowSorting="true" AllowPaging="False">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:BoundField DataField="email" HeaderText="Email" />
                </Columns>
            </asp:GridView>
        </div>
        <div class="modal-footer">
            <asp:Button ID="btnConfirmMerge" runat="server" Text="Yes" CssClass="btn btn-primary" OnClick="btnConfirmMerge_Click" />
            <asp:Button ID="btnCancelMerge" runat="server" Text="No" CssClass="btn btn-primary" OnClick="btnCancelMerge_Click" />
        </div>
    </div>
</asp:Panel>
<h4><asp:Label ID="lblMessage" Text="" runat="server"></asp:Label></h4>
