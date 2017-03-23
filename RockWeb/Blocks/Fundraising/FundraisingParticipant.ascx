﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingParticipant.ascx.cs" Inherits="RockWeb.Blocks.Fundraising.FundraisingParticipant" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server">
            <%-- Main Panel --%>
            <asp:Panel ID="pnlMain" runat="server">
                <asp:HiddenField ID="hfGroupId" runat="server" />
                <asp:HiddenField ID="hfGroupMemberId" runat="server" />
                <asp:HiddenField ID="hfActiveTab" runat="server" />
                <asp:HiddenField ID="hfShareUrl" runat="server" />

                <div class="row">
                    <div class="col-md-4 margin-t-md">
                        <asp:Image ID="imgOpportunityPhoto" runat="server" CssClass="title-image img-responsive" />
                        <asp:LinkButton ID="btnEditPreferences" runat="server" CssClass="btn btn-primary btn-block margin-t-md" Text="Edit Preferences" OnClick="btnEditPreferences_Click" />
                        <asp:LinkButton ID="btnMainPage" runat="server" CssClass="btn btn-primary btn-block margin-t-sm" Text="Main Page" OnClick="btnMainPage_Click" />
                    </div>
                    <div class="col-md-8">
                        
                        <div class="pull-right">
                            <button id="btnCopyToClipboard" runat="server"
                                data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copy Link to Clipboard"
                                class="btn btn-default btn-xs btn-copy-to-clipboard cursor-pointer"
                                onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy Link to Clipboard');return false;">
                                <i class='fa fa-clipboard'></i> Copy Profile Link
                            </button>
                        </div>
                        <asp:Literal ID="lMainTopContentHtml" runat="server" />

                        <Rock:NotificationBox ID="nbProfileWarning" runat="server" Text="A Profile Photo and Summary is recommended. Click Edit Preferences to set these." NotificationBoxType="Success" Visible="false" />
                    </div>
                </div>

                <asp:Panel ID="pnlFundraising" runat="server">
                    <div class="well margin-t-md">
                        <div class="row">
                            <div class="col-md-12">

                                <label>
                                    <asp:Literal ID="lFundraisingProgressTitle" runat="server" Text="Fundraising Progress" />
                                </label>
                                <label class='pull-right'>
                                    <asp:Literal ID="lFundraisingAmountLeftText" runat="server" Text="$320 left" />
                                </label>
                                <asp:Literal ID="lFundraisingProgressBar" runat="server" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <div class="actions pull-right">
                                    <asp:LinkButton ID="btnMakeDonation" runat="server" CssClass="btn btn-sm btn-primary" Text="Contribute to..." OnClick="btnMakeDonation_Click" />
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>



                <div class="row margin-t-md">
                    <asp:Panel ID="pnlUpdatesContributions" CssClass="col-md-8" runat="server">
                        <div class="btn-group">
                            <asp:LinkButton ID="btnUpdatesTab" runat="server" Text="Updates" CssClass="btn btn-default" OnClick="btnUpdatesTab_Click" />
                            <asp:LinkButton ID="btnContributionsTab" runat="server" Text="Contributions" CssClass="btn btn-default" OnClick="btnContributionsTab_Click" />
                        </div>
                        <asp:Panel ID="pnlUpdates" runat="server">
                            <asp:Literal ID="lUpdatesContentItemsHtml" runat="server" />
                        </asp:Panel>
                        <asp:Panel ID="pnlContributions" runat="server">
                            <Rock:Grid ID="gContributions" runat="server" DisplayType="Light" OnRowDataBound="gContributions_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="AuthorizedPersonAlias.Person.FullName" HeaderText="Name" />
                                    <Rock:RockLiteralField ID="lAddress" HeaderText="Address" />
                                    <Rock:DateTimeField DataField="TransactionDateTime" HeaderText="Date" ItemStyle-HorizontalAlign="Left" />
                                    <Rock:CurrencyField DataField="TotalAmount" HeaderText="Amount" HeaderStyle-HorizontalAlign="Right" />
                                </Columns>
                            </Rock:Grid>
                        </asp:Panel>
                    </asp:Panel>
                    <asp:Panel ID="pnlComments" CssClass="col-md-4" runat="server">
                        <label>Comments</label>
                        <Rock:NoteContainer ID="notesCommentsTimeline" runat="server" UsePersonIcon="true" AddAllowed="true" DisplayType="Full" />
                    </asp:Panel>
                </div>
            </asp:Panel>

            <%-- Edit Preferences Panel --%>
            <asp:Panel ID="pnlEditPreferences" runat="server">

                <div class="margin-b-lg">

                    <h1>
                        <asp:Literal ID="lProfileTitle" runat="server" Text="Profile..." /></h1>

                    <asp:Literal ID="lDateRange" runat="server" Text="Dates..." />

                </div>

                <div class="row">

                    <div class="col-md-3">
                        <Rock:ImageEditor ID="imgProfilePhoto" runat="server" Label="Photo" ValidationGroup="vgProfileEdit" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                    </div>

                    <div class="col-md-9">
                        <asp:PlaceHolder ID="phGroupMemberAttributes" runat="server" />
                        <asp:PlaceHolder ID="phPersonAttributes" runat="server" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSaveEditPreferences" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" ValidationGroup="vgProfileEdit" OnClick="btnSaveEditPreferences_Click" />
                    <asp:LinkButton ID="btnCancelEditPreferences" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" ValidationGroup="vgProfileEdit" CausesValidation="false" OnClick="btnCancelEditPreferences_Click" />
                </div>

            </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
