<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditCompanyControl.ascx.cs" Inherits="SWUReporting.EditCompanyControl" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<!-- hidden dummy controls for the modalpopupextender panels -->
<div class="form-inline">
    <asp:Button ID="lnkDummy" runat="server" style="display:none" />
    <asp:Button ID="lnkDummy2" runat="server" style="display:none" />
</div>
    <br />
<cc1:ModalPopupExtender ID="ModalPopupExtender3" BehaviorID="ModalPopupExtender3" runat="server" PopupControlID="pnlPopup" TargetControlID="lnkDummy" BackgroundCssClass="modalBackground" CancelControlID="btnClose">
</cc1:ModalPopupExtender>
<asp:Panel ID="pnlPopup" runat="server" CssClass="modal-dialog-centered modal-xl" style="display:block;" > <!-- was CssClass "panel" -->

        <div class="modal-content">
            <div class="modal-header">
                <h3 class="modal-title">Edit Company</h3>
            </div>
                <asp:UpdatePanel ID="upEditCompany" runat="server">
        <ContentTemplate>
            <div class="modal-body" style="max-height=500px; overflow: auto;">   
                <table class="table table-controls" style="vertical-align: middle">
                    <tr>
                        <td>
                            <p>Company: <asp:TextBox ID="tbCompany" ReadOnly="true" CssClass="form-control" runat="server"></asp:TextBox></p>
                        </td>
                        <td style="vertical-align:bottom;">
                            <p>
                                <asp:CheckBox ID="cbStatus" CssClass="form-control" Text="ACTIVE" runat="server" />
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p>Alias: <asp:TextBox ID="tbAlias" ReadOnly="true" CssClass="form-control" runat="server"></asp:TextBox></p>
                        </td>
                        <td>
                            <!-- company edit button -->
                            <p>VAR Alias: <asp:DropDownList ID="ddVARAlias" runat="server" CssClass="form-control" OnSelectedIndexChanged="ddVARAlias_SelectedIndexChanged"></asp:DropDownList> </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Parent: <asp:TextBox ID="tbParent" ReadOnly="true" CssClass="form-control" runat="server"></asp:TextBox>
                        </td>
                        <td>                            
                            <p>Update Parent:<asp:DropDownList ID="ddVARParent" CssClass="form-control" runat="server" OnSelectedIndexChanged="ddVARParent_SelectedIndexChanged"/></p>                            
                        </td>
                    </tr>
                </table>                                                                                            
            </div>
                    </ContentTemplate>
    </asp:UpdatePanel>
            <div class="modal-footer">                
                <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="modal-close btn btn-primary" OnClick="btnSave_Click"/>                
                <asp:Button ID="btnClose" runat="server" Text="Close" CssClass="modal-close btn" OnClick="btnHide_Click" />
            </div>
        </div>

</asp:Panel>

<asp:UpdatePanel ID="upCompanyList" runat="server">
    <ContentTemplate>
        <asp:GridView ID="gvCompanies" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="false" AllowSorting="true">  <%--AllowPaging="True">--%>
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" />          
                <asp:BoundField DataField="VAR_Alias" HeaderText="VAR Name" />
                <asp:BoundField DataField="VAR_Parent" HeaderText="VAR Parent" />
                <asp:BoundField DataField="status" HeaderText="Status" />
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkEdit" Text="Edit" runat="server" CommandArgument='<%# Eval("ID") %>' OnClick="lnkEdit_Click"/>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        </ContentTemplate>
    </asp:UpdatePanel>
<h4><asp:Label ID="lblMessage" Text="" runat="server"></asp:Label></h4>