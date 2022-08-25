<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditAliasControl.ascx.cs" Inherits="SWUReporting.EditAliasControl" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<!-- hidden dummy controls for the modalpopupextender panels -->
<style type="text/css">
    .auto-style2 {
        width: 221px;
    }
</style>
<div class="form-inline">
    <asp:Button ID="lnkDummy" runat="server" style="display:none" />
    <asp:Button ID="lnkDummy2" runat="server" style="display:none" />
</div>
    <br />
<%-- old code placeholder --%>

<%--  Edit VAR Alias pop-up  --%>
<cc1:ModalPopupExtender ID="ModalPopupExtender3" BehaviorID="mpe3" runat="server" PopupControlID="pnlPopup" TargetControlID="lnkDummy2" BackgroundCssClass="modalBackground" CancelControlID="btnClose">
</cc1:ModalPopupExtender>
<asp:Panel ID="pnlPopup" runat="server" CssClass="modal-dialog-centered modal-xl" style="display:block;" > <!-- was CssClass "panel" -->

        <div class="modal-content">
            <div class="modal-header">
                <%--<h3 class="modal-title">Edit VAR Alias</h3>--%>
                <h3><asp:Label CssClass="modal-title" runat="server" Text="Edit VAR Alias" ID="lblEditAlias"></asp:Label> </h3>
            </div>
            <asp:UpdatePanel runat="server" ID="up1">
                <ContentTemplate>
            <div class="modal-body" style="max-height=500px; overflow: auto;">   
                <table class="table table-controls" style="vertical-align: middle">                    
                    <tr>
                        <td>
                            <asp:CheckBox ID="cbStatus" runat="server" Text="ACTIVE" ToolTip="Dissable or enable the VAR in all reports" />
                        </td>
                        <td style="vertical-align:bottom;" class="auto-style2">
                            <asp:CheckBox ID="cb3DX" runat="server" Text="3DX Contract" ToolTip="The VAR has a 3DEXPERIENCE Contract" />
                        </td>
                        <td style="vertical-align:bottom;">
                            <asp:CheckBox ID="cbDW" runat="server" Text="DW Contract" ToolTip="The VAR has a DELMIAWORKS Contract" />
                        </td>
                        <td style="vertical-align:bottom;">
                            <asp:CheckBox ID="cbEDU" runat="server" Text="EDU VAR" ToolTip="The VAR is EDU only" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p>VAR Alias: <asp:TextBox ID="tbAlias" CssClass="form-control" runat="server" AutoPostBack="true" OnTextChanged="tbAlias_TextChanged"></asp:TextBox></p>
                        </td>
                        <td style="vertical-align:bottom;" class="auto-style2">
                            <p>
                                <asp:CheckBox ID="cbDashboard" runat="server" Text="Dashboard created" ToolTip="VAR Certification Dashboard has been created" />
                            </p>
                        </td>
                        <td style="vertical-align:bottom;">
                            <p>
                                <asp:CheckBox ID="cbDashboardCallMade" runat="server" Text="Dashboard call completed" ToolTip="VAR has been contacted about their dashboard"/>
                            </p>
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>
                            <p>
                                GEO:
                                <asp:TextBox ID="tbGEO" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                            </p>
                        </td>
                        <td class="auto-style2">
                            <!-- company edit button -->
                            <p>
                                GEO:
                                <asp:DropDownList ID="ddGEOs" runat="server" AutoPostBack="true" CssClass="form-control" OnSelectedIndexChanged="ddGEOs_SelectedIndexChanged">
                                </asp:DropDownList>
                            </p>
                        </td>
                        <td></td>
                        <td></td>
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

<%-- Add VAR Parent pop-up --%>
<cc1:ModalPopupExtender ID="mpeParent" BehaviorID="mpeParent" runat="server" PopupControlID="pnlParent" TargetControlID="lnkDummy2" BackgroundCssClass="modalBackground" CancelControlID="btnCloseParent">
</cc1:ModalPopupExtender>
<asp:Panel ID="pnlParent" runat="server" CssClass="modal-dialog-centered modal-xl" style="display:block;" > <!-- was CssClass "panel" -->

        <div class="modal-content">
            <div class="modal-header">
                <%--<h3 class="modal-title">Edit VAR Alias</h3>--%>
                <h3><asp:Label CssClass="modal-title" runat="server" Text="Add VAR Parent" ID="lblAddVARParent"></asp:Label> </h3>
            </div>
            <asp:UpdatePanel runat="server" ID="up2">
                <ContentTemplate>
            <div class="modal-body" style="max-height=500px; overflow: auto;">   
                VAR Parent: <asp:TextBox ID="tbParent" CssClass="form-control" runat="server" OnTextChanged="tbParent_TextChanged" AutoPostBack="true"></asp:TextBox>                                                                                           
            </div>
                
            <div class="modal-footer">                
                <asp:Button ID="btnSaveParent" runat="server" Text="Save" CssClass="modal-close btn btn-primary" OnClick="btnSaveParent_Click"/>                                                    
                <asp:Button ID="btnCloseParent" runat="server" Text="Cancel" CssClass="modal-close btn" OnClick="btnCloseParent_Click" />
            </div>
                            </ContentTemplate>
                <Triggers><asp:PostBackTrigger ControlID="btnSaveParent" /><asp:PostBackTrigger ControlID="btnCloseParent" /></Triggers>
            </asp:UpdatePanel>
        </div>

</asp:Panel>

<div class="form-inline">
    <asp:Button ID="btnAddAlias" runat="server" Text="Add VAR Alias" ToolTip="One VAR Alias per Dashboard" CssClass="btn btn-primary" OnClick="btnAddAlias_Click"/>
    <asp:Button ID="btnAddParent" runat="server" Text="Add VAR Parent" ToolTip="One VAR Parent per Contract (for alignment reporting)" CssClass="btn btn-primary" OnClick="btnAddParent_Click"/>
</div>
    <asp:GridView ID="gvAlias" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="False" AllowSorting="True" OnRowDataBound="gvAlias_RowDataBound">  <%--AllowPaging="True">--%>
        <Columns>   
            <asp:BoundField DataField="VAR_Alias" HeaderText="VAR Name" />
            <asp:BoundField DataField="status" HeaderText="Status" />
            <asp:BoundField DataField="custom_targets" HeaderText="Has Custom Targets" />
            <asp:BoundField DataField="DashboardDate" HeaderText="Has Dashboard" />
            <asp:BoundField DataField="CommunicationDate" HeaderText="Dashboard Call Date" />
            <asp:BoundField DataField="3dx" HeaderText="3DX Contract" />
            <asp:BoundField DataField="DW" HeaderText="DW Contract" />
            <asp:BoundField DataField="EDU" HeaderText="EDU Only" />
            <asp:TemplateField>
                <ItemTemplate>                    
                    <asp:LinkButton CssClass="btn" ID="lnkEdit" Text="Edit" runat="server" CommandArgument='<%# Eval("ID") %>' OnClick="lnkEdit_Click"/>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

<h4><asp:Label ID="lblMessage" Text="" runat="server"></asp:Label></h4>