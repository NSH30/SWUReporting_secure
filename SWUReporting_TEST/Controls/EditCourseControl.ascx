<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditCourseControl.ascx.cs" Inherits="SWUReporting.EditCourseControl" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc2" %>
<div class="form-inline">
    <asp:LinkButton ID="lnkSaveExcel" CssClass="btn btn-primary" runat="server" OnClick="lnkSaveExcel_Click" style="margin-bottom:10px" ToolTip="Download">
        <span aria-hidden="true" class="glyphicon glyphicon-download-alt"></span>
    </asp:LinkButton>
</div>
<div class="container" style="margin-top:10px">
    <asp:UpdatePanel runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>
        <h4>Courses</h4>
        <asp:GridView ID="gvCourses" cssclass="table table-striped table-bordered" runat="server"  AutoGenerateColumns="false" Visible ="true">
            <Columns>                
                <asp:BoundField DataField="Name" HeaderText="Course Name" />
                <asp:BoundField DataField="Type"  HeaderText="Course Type" />
                <asp:BoundField DataField="CourseNameAlias" HeaderText="Course Alias"/>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:LinkButton CssClass="btn" ID="lnkEditCourse" Text="Edit Mapping" runat="server" CommandArgument='<%# Eval("ID") %>' OnClick="lnkEditCourse_Click" />
                    </ItemTemplate>                        
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>

<cc2:ModalPopupExtender ID="ModalPopupExtender3" BehaviorID="mpe1" runat="server" PopupControlID="pnlPopup" TargetControlID="lnkDummy2" BackgroundCssClass="modalBackground" CancelControlID="btnClose">
</cc2:ModalPopupExtender>
<asp:Panel ID="pnlPopup" runat="server" CssClass="modal-dialog-centered modal-xl" style="display:block;" > <!-- was CssClass "panel" -->

        <div class="modal-content">
            <div class="modal-header">                
                <h3><asp:Label CssClass="modal-title" runat="server" Text="Edit Course Mapping" ID="lblEditCourse"></asp:Label> </h3>
            </div>
            <asp:UpdatePanel runat="server" ID="up1">
                <ContentTemplate>
            <div class="modal-body" style="max-height=500px; overflow: auto;">   
                <table class="table table-controls" style="vertical-align: middle">                    
                    <tr>
                        <td colspan="2">
                            Course Name: <asp:TextBox ID="tbCourse" CssClass="form-control" runat="server" AutoPostBack="true" ReadOnly="true"></asp:TextBox>
                        </td>

                    </tr>
                    <tr>
                        <td>                            
                             <p>Course Alias: <asp:TextBox ID="tbAlias" CssClass="form-control" runat="server" ReadOnly="true"></asp:TextBox></p>
                        </td>
                        <td>
                            <p>New Alias: <asp:DropDownList ID="ddAlias" runat="server" AutoPostBack="true" CssClass="form-control" OnSelectedIndexChanged="ddAlias_SelectedIndexChanged">
                                </asp:DropDownList>
                                </p>
                        </td>
                    </tr>
                </table>                                                                                            
            </div>
                
            <div class="modal-footer">                
                <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="modal-close btn btn-primary" OnClick="btnSave_Click"/>                                                    
                <asp:Button ID="btnClose" runat="server" Text="Cancel" CssClass="modal-close btn" OnClick="btnClose_Click" />
            </div>
                            </ContentTemplate>
                <Triggers><asp:PostBackTrigger ControlID="btnSave" /><asp:PostBackTrigger ControlID="btnClose" /></Triggers>
            </asp:UpdatePanel>
        </div>

</asp:Panel>
