 define("BaseIntroPageSchema", ["BaseIntroPageSchemaResources","MsTeamsMainMenuTileGenerator"], 
function(resources) {
	return {
		methods:{
			navigateToSalesloft: function(){

				let helpLink = this.Terrasoft.getFileContentUrl("MicrosoftTeams","Documentation/index.html");
				window.open(helpLink);
			},
			/**
			 * @inheritdoc Terrasoft.BasePageV2#onRender
			 * @override
			 */
			 onRender: function() {
				var isMarketPlacePanelVisible = this.getIsMarketPlacePanelVisible();
				if (!this.getIsMarketPlacePanelVisible()) {
					this._marketPlaceIframeConfigure();
				}
				var sdkContainer = this.Ext.get("sdk-container-el");
				var gettingStartedContainer = this.Ext.get("gettingStarted-container-el");
				var communityContainer = this.Ext.get("community-container-el");
				var marketplaceContainer = isMarketPlacePanelVisible ?
						this.Ext.get("marketplace-container-el") :
						null;
				var academyContainer = this.Ext.get("academy-container-el");
				var salesloftContainer = this.Ext.get("salesloft-container-el");
				if (this.isNotEmpty(sdkContainer)) {
					sdkContainer.on("click", this.SdkClick, this);
				}
				if (this.isNotEmpty(gettingStartedContainer)) {
					gettingStartedContainer.on("click", this.navigateToGettingStarted, this);
				}
				if (this.isNotEmpty(communityContainer)) {
					communityContainer.on("click", this.CommunityClick, this);
				}
				if (isMarketPlacePanelVisible && this.isNotEmpty(marketplaceContainer)) {
					marketplaceContainer.on("click", this.navigateToMarketplace, this);
				}
				if (this.isNotEmpty(academyContainer)) {
					academyContainer.on("click", this.navigateToAcademy, this);
				}
				if (this.isNotEmpty(salesloftContainer)) {
					salesloftContainer.on("click", this.navigateToSalesloft, this);
				}
				Terrasoft.defer(function() {
					this._initLmsDocumentations();
					this._initWidget();
				}, this);
			},
		},
		diff:[
			{
				"operation": "merge",
				"name": "TerrasoftAccountsLinksPanel",
				"propertyName": "items",
				"parentName": "LinksContainer",
				"values": {
					"generator": "MsTeamsMainMenuTileGenerator.generateTerrasoftAccountsLinks",
					"salesloftIcon": resources.localizableImages.MsTeamsIcon,
					"salesloftCaption": "Microsoft Graph connector for Creatio documentation",
					"salesloftLabel": "Microsoft Teams",
				}
			}
		]
	};
});
