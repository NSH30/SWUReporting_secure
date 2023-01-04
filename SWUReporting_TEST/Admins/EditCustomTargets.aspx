<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditCustomTargets.aspx.cs" Inherits="SWUReporting_TEST.EditCustomTargets" MasterPageFile="~/Site.Master" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<%-- This functionality is not used - Mike Spens July 25, 2022 --%>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <br />
    <table style="width:100%">
        <tr>
            <td  style="width:80%">
                <asp:Panel CssClass="form-inline" ID="CompanySearch" runat="server" DefaultButton="lnkSearch">
                    <asp:TextBox ID="tbSearch" runat="server" CssClass="form-control" Width="50%"  placeholder="Search VARs..."/>
                    <%--<asp:Button ID="btnSearch" runat="server" CssClass="btn" OnClick="btnSearch_Click" Text="Search"  />--%>
                    <asp:LinkButton runat="server" ID="lnkSearch" CssClass="btn form-inline" placeholder="Search" OnClick="btnSearch_Click">
                        <span aria-hidden="true" class="btn glyphicon glyphicon-search" />
                    </asp:LinkButton>
                </asp:Panel>
            </td>
        </tr>
    </table>

    <div class="container">
        <h4>Search Results</h4>
        <asp:GridView ID="gvAlias" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="false" AllowSorting="true" Visible="false">  <%--AllowPaging="True">--%>
            <Columns>   
                <asp:BoundField DataField="VAR_Alias" HeaderText="VAR Name" />
<%--                <asp:BoundField DataField="status" HeaderText="Status" />--%>
                <asp:BoundField DataField="custom_targets" HeaderText="Has Custom Targets" />
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:LinkButton CssClass="btn" ID="lnkTargets" Text="Edit/Add Custom Targets" runat="server" CommandArgument='<%# Eval("ID") %>' OnClick="lnkTargets_Click" />                    
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
    <br />
    <div class="container">
        <asp:UpdatePanel runat="server">
            <%--<Triggers><asp:PostBackTrigger ControlID="lnkCustomTarget" /></Triggers>--%>
            <ContentTemplate>
            <h4>Edit Custom Targets</h4>
            <asp:GridView ID="gvTargets" cssclass="table table-striped table-bordered" runat="server"  AutoGenerateColumns="false" Visible ="false">
                <Columns>                
                    <asp:BoundField DataField="CourseNameAlias" HeaderText="Course" />
                    <asp:BoundField DataField="Default Target" DataFormatString="{0:0%}" HeaderText="Default Target" />
                    <asp:BoundField DataField="Custom Target" DataFormatString="{0:0%}" HeaderText="Custom Target (%)" />
                    <asp:BoundField DataField="FTETarget" HeaderText="Custom FTE Target" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton CssClass="btn" ID="lnkCustomTarget" Text="Edit" runat="server" CommandArgument='<%# Eval("ID") + ";" + Eval("courseID") + ";" + Eval("FTETarget") %>' OnClick="lnkCustomTarget_Click" />                    
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <div class="form-inline">
    <asp:Button ID="lnkDummy" runat="server" style="display:none" />
    </div>
    <cc1:ModalPopupExtender ID="mpe9" BehaviorID="mpe9" runat="server" PopupControlID="pnlEdit" TargetControlID="lnkDummy" BackgroundCssClass="modalBackground" CancelControlID="btnClose">
    </cc1:ModalPopupExtender>
    <asp:Panel ID="pnlEdit" runat="server" CssClass="modal-dialog-centered modal-sm">        
        <asp:UpdatePanel ID="UpdatePanel2" ChildrenAsTriggers="true" runat="server">
                <Triggers><asp:PostBackTrigger ControlID="lnkEditCT" /><asp:PostBackTrigger ControlID="btnClose" /></Triggers>
                <ContentTemplate>
                <div class="modal-content">
                    <div class="modal-header">
                        <h4>Edit Custom Targets</h4>
                    </div>
                    <div class="modal-body" style="overflow:auto">
                        <table class="table table-controls" style="vertical-align: middle;">
                            <tr>
<%--                                <td class="form-inline">
                                    <asp:Label ID="lblCustomTarget" runat="server" >Custom Target: </asp:Label>
                                    <asp:TextBox ID="tbCustomTarget" CssClass="input" runat="server"></asp:TextBox>
                                </td>--%>
                                <td class="form-inline">
                                    <asp:Label ID="lblFTETarget" runat="server" >Custom FTE Target: </asp:Label>
                                    <asp:TextBox ID="tbCustomFTETarget" CssClass="input" runat="server"></asp:TextBox>
                                </td>
                            </tr>
                            <tr style="text-align:right">
                                <%--<td></td>--%>
                                <td>
                                    <asp:LinkButton ID="lnkEditCT" CssClass="btn btn-primary" runat="server" OnClick="lnkEditCT_Click" Text="Save"/>
                                    <asp:LinkButton ID="btnClose" CssClass="btn btn-default" runat="server" OnClick="btnClose_Click" Text="Cancel"/>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
                </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
</asp:Content>
    

