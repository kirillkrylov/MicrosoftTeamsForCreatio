  define("SystemDesigner", ["RightUtilities","ServiceHelper"], function(RightUtilities, ServiceHelper) {
	return {
		attributes: {		
			"oAuthLink": {
				dataValueType: this.Terrasoft.DataValueType.TEXT,
				value: ""
			},

		},
		methods: {
			/**
			 * Open the chat settings window.
			 * @protected
			 */
			navigateToMsTeamsOAuth: function() {
				var url = Ext.String.format(
					"{0}", 
					this.$oAuthLink)
				window.open(url, "_blank");
			},


			/**
			 * @inheritdoc Terrasoft.BaseSchemaViewModel#init
			 * @overridden
			 */
			init: function() {
				this.callParent(arguments);
				this.getOAuthLink();
			},


			getOAuthLink: function(){

				ServiceHelper.callService(
					"OAuthHandlerService",			//CS - ClassName
					"GetLoginUrl", 					//CS - Method
					function(response) 
					{
						var result = response;
						if(result){
							this.$oAuthLink = result.LoginUrl;
						}
					}, 
					"", 
					this
				);
			},
		},
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"propertyName": "items",
				"parentName": "IntegrationTile",
				"name": "SalesLoftConnector",
				"values": {
					"itemType": Terrasoft.ViewItemType.LINK,
					"caption": {"bindTo": "Resources.Strings.MsTeamsConnectorCaption"},
					"tag": "navigateToMsTeamsOAuth",
					"click": { "bindTo": "invokeOperation" }
				}
			}
		]/**SCHEMA_DIFF*/
	};
});
