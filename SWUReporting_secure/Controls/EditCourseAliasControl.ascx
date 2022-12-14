<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditCourseAliasControl.ascx.cs" Inherits="SWUReporting.EditCourseAliasControl" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
    <asp:Button ID="btnAddCourse" runat="server" CssClass="btn btn-primary" OnClick="btnAddCourse_Click" Text="Add Course" />
    <div class="container" style="margin-top:10px">
        <asp:UpdatePanel runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
            <ContentTemplate>
            <h4>Course Alias Details</h4>
            <asp:GridView ID="gvAlignment" cssclass="table table-striped table-bordered" runat="server"  AutoGenerateColumns="false" Visible ="false">
                <Columns>                
                    <asp:BoundField DataField="CourseNameAlias" HeaderText="Course" />
                    <asp:BoundField DataField="alignment_points"  HeaderText="Alignment Points" />
                    <asp:BoundField DataField="Target" HeaderText="Default Target" DataFormatString="{0:0%}"/>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton CssClass="btn" ID="lnkEditAlignment" Text="Edit" runat="server" CommandArgument='<%# Eval("ID") %>' OnClick="lnkEditAlignment_Click" />
                        </ItemTemplate>                        
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton CssClass="btn" ID="lnkShowCourses" Text="Show Courses" runat="server" CommandArgument='<%# Eval("ID") %>' OnClick="lnkShowCourses_Click" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <!-- add a new course alias, mapped to an existing course -->
    <div class="form-inline">
        <asp:Button ID="lnkDummy2" runat="server" style="display:none" />
    </div>
    <cc1:ModalPopupExtender ID="mpe1" BehaviorID="mpe1" runat="server" PopupControlID="pnlAddCourse" TargetControlID="lnkDummy2" BackgroundCssClass="modalBackground" CancelControlID="btnCloseCourse">
    </cc1:ModalPopupExtender>
    <asp:Panel ID="pnlAddCourse" runat="server" CssClass="modal-dialog-centered modal-lg" style="display:block;">
        <div class="modal-content">
            <div class="modal-header">
            <h4>Add Course Alias</h4>
            </div>
            <asp:UpdatePanel ID="up1" ChildrenAsTriggers="true" runat="server">
                <Triggers><asp:PostBackTrigger ControlID="lnkSaveCourse" /><asp:PostBackTrigger ControlID="btnCloseCourse" /></Triggers>
                <ContentTemplate>
                    <div class="modal-body" style="overflow:auto">
                        <table class="table table-controls form-inline" style="vertical-align: middle">
                            <tr>
                                <td>
                                    <asp:Label ID="lblNewCourseName" runat="server" >Course Name: </asp:Label>
                                </td>
                                <td>
                                    <asp:TextBox ID="tbNewCourseName" runat="server" Text=""></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblNewDefaultTarget" runat="server" >Default Target: </asp:Label>
                                </td>
                                <td>
                                    <asp:TextBox ID="tbNewDefaultTarget" runat="server" Text="" ToolTip="Enter a value between 0 (no target) and 1 (all FTEs target)"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblNewAlignmentPoints" runat="server" >Alignment Points: </asp:Label>
                                </td>
                                <td>
                                    <asp:TextBox ID="tbNewAlignmentPoints" runat="server" Text=""></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblCourseMapping" runat="server" >Course Mapping: </asp:Label>
                                </td>
                                <td>
                                    <!-- dropdown list of all courses -->
                                    <asp:DropDownList ID="ddCourses" runat="server" CssClass="form-control"></asp:DropDownList>
                                 </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblDomain" runat="server" >Domain: </asp:Label>
                                </td>
                                <td>
                                    <!-- dropdown list of all domains -->
                                    <asp:DropDownList ID="ddDomains" runat="server" CssClass="form-control"></asp:DropDownList>
                                 </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td style="text-align:right">
                                    <asp:LinkButton ID="lnkSaveCourse" CssClass="btn btn-primary" runat="server" OnClick="lnkSaveCourse_Click" Text="Save"/>
                                    <asp:LinkButton ID="btnCloseCourse" CssClass="btn btn-default" runat="server" OnClick="btnCloseCourse_Click" Text="Cancel"/>
                                </td>
                            </tr>
                        </table>
                    </div>
                    </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>

<!-- view related courses -->

    <div class="form-inline">
        <asp:Button ID="lnkDummyRC" runat="server" style="display:none" />
    </div>
    <cc1:ModalPopupExtender ID="mpe11" BehaviorID="mpe11" runat="server" PopupControlID="pnlShowCourses" TargetControlID="lnkDummyRC" BackgroundCssClass="modalBackground" CancelControlID="btnCloseShowCourses">
    </cc1:ModalPopupExtender>
    <asp:Panel ID="pnlShowCourses" runat="server" CssClass="modal-dialog-centered modal-xl" style="display:block;">
        <div class="modal-content">
            <div class="modal-header">
            <h4>Mapped Courses</h4>
            </div>
            <asp:UpdatePanel ID="upCourses" ChildrenAsTriggers="true" runat="server">
                <ContentTemplate>
                    <div class="modal-body" style="max-height: 500px; overflow: auto;">
                        <asp:Label ID="lblMessage" runat="server" Text="No mapped courses." CssClass="label label-primary" Visible="false"></asp:Label>
                        <asp:GridView ID="gvCourses" CssClass="table table-striped table-bordered" runat="server" AutoGenerateColumns="false" Visible="true" Width="599px">
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="Course" />
                                <asp:BoundField DataField="Type" HeaderText="Type" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
            <div class="modal-footer">    
                <asp:LinkButton ID="btnCloseShowCourses" CssClass="btn btn-default" runat="server" Text="Close"/>
            </div>
        </div>
    </asp:Panel>

    <!-- edit an existing course alias -->
    <div class="form-inline">
        <asp:Button ID="lnkDummy" runat="server" style="display:none" />
    </div>
    <cc1:ModalPopupExtender ID="mpe8" BehaviorID="mpe8" runat="server" PopupControlID="pnlEdit" TargetControlID="lnkDummy" BackgroundCssClass="modalBackground" CancelControlID="btnClose">
    </cc1:ModalPopupExtender>
    <asp:Panel ID="pnlEdit" runat="server" CssClass="modal-dialog-centered modal-sm" OnPreRender="pnlEdit_PreRender" style="display:block;">
        <div class="modal-content">
            <div class="modal-header">
            <h4>Edit Alignment Points</h4>
            </div>
            <asp:UpdatePanel ID="UpdatePanel2" ChildrenAsTriggers="true" runat="server">
                <Triggers><asp:PostBackTrigger ControlID="lnkSave" /><asp:PostBackTrigger ControlID="btnClose" /></Triggers>
                <ContentTemplate>
                    <div class="modal-body" style="overflow:auto">
                        <table class="table table-controls form-inline" style="vertical-align: middle">
                            <tr>
                                <td>
                                    <asp:Label ID="lblCourseName" runat="server" >Course Name: </asp:Label>
                                </td>
                                <td>
                                    <asp:TextBox ID="tbCourseName" runat="server" Text=""></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblTarget" runat="server" >Default Target: </asp:Label>
                                </td>
                                <td>
                                    <asp:TextBox ID="tbTarget" runat="server" Text=""></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblAlignment" runat="server" >Alignment Points: </asp:Label>
                                </td>
                                <td>
                                    <asp:TextBox ID="tbAlignment" runat="server" Text=""></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblKPI" runat="server" >Type: </asp:Label>
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddKPI" runat="server"></asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td style="text-align:right">
                                    <asp:LinkButton ID="lnkSave" CssClass="btn btn-primary" runat="server" OnClick="lnkSave_Click" Text="Save"/>
                                    <asp:LinkButton ID="btnClose" CssClass="btn btn-default" runat="server" OnClick="btnClose_Click" Text="Cancel"/>
                                </td>
                            </tr>
                        </table>
                    </div>
                    </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>