<%@ Page Language="C#" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" CodeBehind="EditCompanies.aspx.cs" Inherits="SWUReporting_TEST.EditCompanies" MasterPageFile="~/Site.Master" %>
<%@ Register Src="~/Controls/EditCompanyControl.ascx" TagPrefix="uc5" TagName="ECC" %>
<%@ Register Src="~/Controls/EditAliasControl.ascx" TagPrefix="uc5" TagName="EditAliasControl" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">        
    <!-- <div class="form-inline">-->
    <br />
<%--    <asp:UpdatePanel ID="upTop" runat="server">
        <ContentTemplate>--%>
            <table style="width:100%">
                <tr>
                    <td  style="width:50%">
                        <asp:Panel CssClass="form-inline" runat="server" ID="CompanySearch" DefaultButton="lnkSearch">
                            <asp:TextBox ID="tbSearch" runat="server" CssClass="form-control" Width="50%" Placeholder="Search"/>
                            <%--<asp:Button ID="btnSearch" runat="server" CssClass="btn" OnClick="btnSearch_Click" Text="Search" />--%>
                            <asp:LinkButton runat="server" ID="lnkSearch" CssClass="btn form-inline" placeholder="Search" OnClick="btnSearch_Click">
                                <span aria-hidden="true" class="btn glyphicon glyphicon-search" />
                            </asp:LinkButton>
                        </asp:Panel>
                    </td>
                    <td>
                        <div class="form-inline">
                        <asp:Button ID="btnNotMapped" runat="server" CssClass="btn" Text="Show Not Mapped" OnClick="btnNotMapped_Click" />
                        </div>
                    </td>
                    <td>
                        <div class="form-inline">
                        <asp:Button ID="btnAlias" runat="server" CssClass="btn" Text="VAR Alias List" OnClick="btnAlias_Click" />
                        <asp:DropDownList ID="ddGEOs" CssClass="form-control" runat="server" style="width:auto" AutoPostBack="true"></asp:DropDownList>
                        </div>
                    </td>
                </tr>
            </table>
<%--            </ContentTemplate>
    </asp:UpdatePanel>--%>
<br />
<div class="form-inline">
    <asp:Button ID="lnkDummy" runat="server" style="display:none" />
    <asp:Button ID="lnkDummy2" runat="server" style="display:none" />
</div>
    <br />
    <uc5:ECC runat="server" ID="ecc2" Visible="false"/>
    <uc5:EditAliasControl runat="server" ID="EditAliasControl" Visible="false" />
</asp:Content> 
