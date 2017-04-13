<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkImportCleanup.ascx.cs" Inherits="RockWeb.Blocks.Utility.BulkImportCleanup" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-eraser"></i>&nbsp;Bulk Import Cleanup</h1>
            </div>

            <div class="panel-body">
                <p>
                    <asp:LinkButton ID="btnCleanupFinancial" runat="server" CssClass="btn btn-default" Text="Cleanup Financial Transaction Import" OnClick="btnCleanupFinancial_Click" /></p>
                <p>
                    <asp:LinkButton ID="btnDeleteAttendance" runat="server" CssClass="btn btn-default" Text="Delete all Attendance" OnClick="btnDeleteAttendance_Click" /></p>
                <p>
                    <asp:LinkButton ID="btnCleanupPerson" runat="server" CssClass="btn btn-default" Text="Cleanup Person Import" OnClick="btnCleanupPerson_Click" /></p>
                <p>
                    <asp:LinkButton ID="btnCleanupEverythingElse" runat="server" CssClass="btn btn-default" Text="Delete imported Groups, Schedules, Locations" OnClick="btnCleanupEverythingElse_Click" /></p>

                <Rock:NotificationBox ID="nbResults" runat="server" NotificationBoxType="Success" Dismissable="true" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
