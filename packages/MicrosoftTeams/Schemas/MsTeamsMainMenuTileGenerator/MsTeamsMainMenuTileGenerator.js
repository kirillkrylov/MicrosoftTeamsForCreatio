define("MsTeamsMainMenuTileGenerator", ["ext-base", "terrasoft", "ModuleUtils", "ViewGeneratorV2"
], function(Ext, Terrasoft, ModuleUtils) {

	Ext.define("Terrasoft.configuration.MsTeamsMainMenuTileGenerator", {
		extend: "Terrasoft.configuration.ViewGenerator",
		alternateClassName: "Terrasoft.MsTeamsMainMenuTileGenerator",

		/**
		 * Generates configuration view for {Terrasoft.ContainerList}.
		 * @protected
		 * @virtual
		 * @param {Object} config Element configuration.
		 * @return {Object} Returns generated view for {Terrasoft.ContainerList}.
		 */
		generateMainMenuTile: function(config) {
			var clonedConfig = Terrasoft.deepClone(config);
			var result = this.generateTileView(clonedConfig);
			return result;
		},

		/**
		 * Generates view of tile.
		 * @protected
		 * @virtual
		 * @param {Object} config Tile configuration.
		 * @return {Object} Returns generated view of tile.
		 */
		generateTileView: function(tile) {
			var tileId = "tile-" + tile.name;
			var tileItemsContainerCfg = {
				className: "Terrasoft.Container",
				classes: {
					wrapClassName: ["items-container"]
				},
				items: []
			};
			var iconContainer = {
				className: "Terrasoft.Container",
				classes: {
					wrapClassName: ["image-panel"]
				}
			};
			if (tile.icon) {
				var iconUrl = Terrasoft.ImageUrlBuilder.getUrl(tile.icon);
				iconContainer.styles = {
					"wrapStyles": {"background-image": "url(" + iconUrl + ")"}
				};
			}
			var items = this.generateTileItems(tile.items);
			tileItemsContainerCfg.items = items;
			var wrapClasses = ["tile"];
			if (!Ext.isEmpty(tile.cls)) {
				var customClasses = Ext.isArray(tile.cls) ? tile.cls : [tile.cls];
				wrapClasses = wrapClasses.concat(customClasses);
			}
			var tileContainer = {
				className: "Terrasoft.Container",
				id: tileId,
				classes: {
					wrapClassName: wrapClasses
				},
				markerValue: tile.markerValue || tile.caption,
				items: [
					{
						className: "Terrasoft.Container",
						classes: {
							wrapClassName: ["view-panel"]
						},
						items: [
							{
								className: "Terrasoft.Container",
								id: tileId + "-color-panel",
								items: [
									{
										className: "Terrasoft.ImageView",
										imageSrc: Terrasoft.ImageUrlBuilder.getUrl(tile.icon),
										classes: {
											wrapClass: ["image-panel"]
										}
									},
									{
										className: "Terrasoft.Label",
										id: tileId + "-caption",
										caption: tile.caption,
										classes: {
											labelClass: ["tile-caption"]
										}
									}
								]
							},
							{
								className: "Terrasoft.Container",
								classes: {
									wrapClassName: ["content-panel"]
								},
								items: [
									tileItemsContainerCfg
								]
							}
						]
					}
				]
			};
			if (!Ext.isEmpty(tile.visible)) {
				tileContainer.visible = tile.visible;
			}
			return tileContainer;
		},

		/**
		 * Generate link label template.
		 * @protected
		 * @param {config} config Template configuration.
		 * @param {String} config.code Label code.
		 * @param {String} config.icon Label icon.
		 * @param {String} config.label Label caption.
		 * @throws {Terrasoft.ArgumentNullOrEmptyException} If config argument is empty.
		 * @return {Array} Link label template.
		 */
		generateTerrasoftLinkLabelTemplate: function(config) {
			if (!config) {
				throw new Terrasoft.ArgumentNullOrEmptyException({argumentName: "config"});
			}
			var code = config.code;
			var icon = config.icon;
			var label = config.label;
			var format = Ext.String.format;
			return [
				format("<a id=\"{0}-link\" class=\"{0}-link menu-link\">", code),
				format("<img id=\"{0}-icon\" class=\"{0}-icon\" src=\"{1}\"/>", code, icon),
				format("<label id=\"{0}-label\" class=\"{0}-label\">{1}</label>", code, label),
				"</a>"
			];
		},

		/**
		 * Generate link container config.
		 * @protected
		 * @param {config} config Configuration.
		 * @param {String} config.code Container code.
		 * @param {Boolean} config.visible Container visible.
		 * @param {String} config.caption Caption.
		 * @param {Array} config.labelTemplate Label template.
		 * @param {String} config.markerValue Marker value.
		 * @throws {Terrasoft.ArgumentNullOrEmptyException} If config argument is empty.
		 * @return {Object} Link container config.
		 */
		generateTerrasoftLinkContainerConfig: function(config) {
			if (!config) {
				throw new Terrasoft.ArgumentNullOrEmptyException({argumentName: "config"});
			}
			var code = config.code;
			var format = Ext.String.format;
			return {
				className: "Terrasoft.Container",
				classes: {
					wrapClassName: [format("{0}-container", code)]
				},
				visible: config.visible,
				id: format("{0}-container-el", code),
				items: [
					{
						className: "Terrasoft.Label",
						classes: {
							labelClass: [format("{0}-caption", code), "menu-caption"]
						},
						caption: config.caption
					},
					{
						className: "Terrasoft.Component",
						tpl: config.labelTemplate,
						selectors: {
							wrapEl: format("#{0}-icon", code)
						}
					}
				],
				markerValue: config.markerValue
			};
		},

		/**
		 * Generate title items.
		 * @protected
		 * @param {Array} itemsCfg Title items config.
		 * @return {Array} Title items.
		 */
		generateTileItems: function(itemsCfg) {
			var result = [];
			Terrasoft.each(itemsCfg, function(item) {
				var tileItemContainer = {
					className: "Terrasoft.Container",
					classes: {
						wrapClassName: ["tile-item"]
					},
					visible: (item.visible === undefined) ? true : item.visible,
					items: []
				};
				var moduleName = item.moduleName;
				var tag = item.tag;
				var caption = item.caption;
				if (!Ext.isEmpty(moduleName)) {
					var module = ModuleUtils.getModuleStructureByName(moduleName);
					tag = tag || ModuleUtils.getModuleTag(moduleName);
					caption = caption || (module && module.moduleCaption);
				}
				var tileItemView =
					{
						className: "Terrasoft.Hyperlink",
						caption: caption,
						markerValue: item.markerValue || caption,
						tag: tag
					};
				if (item.click) {
					tileItemView.click = item.click;
				}
				tileItemContainer.items.push(tileItemView);
				result.push(tileItemContainer);
			}, this);
			return result;
		},

		/**
		 * Generate video panel config.
		 * @param {Object} config Video panel config.
		 * @param {Object} config.playBtnIcon Play button config.
		 * @param {Object} config.activePlayBtnIcon Active play button config.
		 * @param {Object} config.playlist Playlist.
		 * @param {Object} config.markerValue Marker value.
		 * @return {Object} Video panel config.
		 */
		generateVideoPanel: function(config) {
			var playBtnIcon = "";
			var activePlayBtnIcon = "";
			var defaultVideoUrl = "";
			if (config.loadVideo !== false) {
				playBtnIcon = Terrasoft.ImageUrlBuilder.getUrl(config.playBtnIcon);
				activePlayBtnIcon = Terrasoft.ImageUrlBuilder.getUrl(config.activePlayBtnIcon);
			}
			var playlist = this.generatePlayList(config.playlist, playBtnIcon, activePlayBtnIcon);
			if (config.loadVideo !== false) {
				defaultVideoUrl = playlist.length > 0 ? playlist[0].href : "";
			}
			var result = {
				className: "Terrasoft.Container",
				classes: {
					wrapClassName: ["video-container", "right-item"]
				},
				"id": "VideoPanel",
				"selectors": {"wrapEl": "#VideoPanel"},
				markerValue: config.markerValue || "video-container",
				items: [
					{
						className: "Terrasoft.Container",
						html: "<iframe name=\"bpmonline-video\" id=\"bpmonline-video\" width=\"560\" height=\"315\"" +
						" src=\"" + encodeURI(defaultVideoUrl) + "\" frameborder=\"0\" allowfullscreen></iframe>",
						selectors: {
							wrapEl: "#bpmonline-video"
						}
					}
				]
			};
			result.items = result.items.concat(playlist);
			result.visible = (playlist.length > 0);
			return result;
		},

		/**
		 * Generate playlist.
		 * @param {Array} itemsCfg Items config.
		 * @param {String} playBtnIcon Play button icon.
		 * @return {Array} Playlist.
		 */
		generatePlayList: function(itemsCfg, playBtnIcon) {
			var playlist = [];
			Terrasoft.each(itemsCfg, function(item) {
				if (!Ext.isEmpty(item.videoUrl)) {
					var playlistItem = {
						className: "Terrasoft.Hyperlink",
						tpl: [
							"<a id=\"{id}\" name=\"{name}\" href=\"{href}\" target=\"{target}\"" +
							" class=\"{hyperlinkClass}\" style=\"{hyperlinkStyle}\" title=\"{hint}\" type=\"{type}\">",
							"<div class=\"{videoLinkImageClass}\" style=\"{playBtnStyle}\"></div>{caption}",
							"</a>"
						],
						target: "bpmonline-video",
						href: item.videoUrl,
						caption: item.caption,
						classes: {
							hyperlinkClass: ["ts-box-sizing", "video-link"],
							videoLinkImageClass: ["play-video-image"]
						},
						tplData: {
							playBtnStyle: {
								"background-image": "url(" + playBtnIcon + ")"
							}
						}
					};
					playlist.push(playlistItem);
				}
			}, this);
			return playlist;
		},

		/**
		 * Generate mobile banner container config.
		 * @param {Object} config Academy banner config.
		 * @param {Object} config.androidIcon Android icon config.
		 * @param {String} config.androidUrl Android app url.
		 * @param {String} config.androidLabel Android app label.
		 * @param {Object} config.iosIcon iOS icon config.
		 * @param {String} config.iosUrl iOS app url.
		 * @param {String} config.iosLabel iOS app label.
		 * @param {Boolean} config.visible Banner container config.
		 * @param {String} config.markerValue Marker value.
		 * @param {String} config.caption Banner container caption.
		 * @return {Object} Mobile banner container config.
		 */
		generateMobileAppBaner: function(config) {
			var itemsInfo = [
				{
					id: "app-store",
					storeLink: config.iosUrl,
					icon: config.iosIcon,
					label: config.iosLabel
				},
				{
					id: "play-market",
					storeLink: config.androidUrl,
					icon: config.androidIcon,
					label: config.androidLabel
				}
			];
			var items = itemsInfo.map(function(item) {
				return {
					className: "Terrasoft.Component",
					tpl: [
						"<a id=\"" + item.id + "\" class=\"{linkClass}\" href=\"{storeLink}\" target=\"_blank\" rel=\"nofollow\">",
						"<img src=\"{imageUrl}\" class=\"{imageClass}\" alt=\"Download on the " + item.label + "\">",
						"<span>{label}</span>",
						"</a>"
					],
					tplData: {
						storeLink: item.storeLink,
						imageUrl: Terrasoft.ImageUrlBuilder.getUrl(item.icon),
						label: item.label
					},
					classes: {
						linkClass: ["mobile-app-link", "menu-link"],
						imageClass: ["mobile-app-image"]
					},
					selectors: {wrapEl: "#" + item.id},
					markerValue: item.id
				};
			});
			var result = {
				className: "Terrasoft.Container",
				classes: {
					wrapClassName: ["mobile-apps-container", "right-item"]
				},
				visible: config.visible,
				markerValue: config.markerValue || "mobile-apps-container",
				items: [
					{
						className: "Terrasoft.Label",
						caption: config.caption
					},
					{
						className: "Terrasoft.Container",
						classes: {wrapClassName: ["mobile-apps-links-container"]},
						items: items
					}
				]
			};
			return result;
		},

		/**
		 * Generate accounts links container config.
		 * @param {Object} config Accounts links config.
		 * @param {Array} config.socialAccounts Social accounts config.
		 * @param {String} config.wrapClassName Wrap class name.
		 * @param {String} config.markerValue Marker value.
		 * @param {Object} config.gettingStartedIcon Getting started icon config.
		 * @param {String} config.gettingStartedLabel Getting started label.
		 * @param {Boolean} config.isGettingStartedVisible Getting started link visible.
		 * @param {String} config.gettingStartedCaption Getting started caption.
		 * @param {Object} config.communityIcon Community icon config.
		 * @param {String} config.communityLabel Academy label.
		 * @param {Boolean} config.IsComunityPanelVisible Academy link visible.
		 * @param {String} config.communityCaption Academy caption.
		 * @param {Object} config.marketplaceIcon Marketplace icon config.
		 * @param {String} config.marketplaceLabel Academy label.
		 * @param {Boolean} config.isMarketplacePanelVisible Academy link visible.
		 * @param {String} config.marketplaceCaption Academy caption.
		 * @param {Object} config.academyIcon Academy icon config.
		 * @param {String} config.academyLabel Academy label.
		 * @param {Boolean} config.isAcademyVisible Academy link visible.
		 * @param {String} config.academyCaption Academy caption.
		 * @param {Boolean} config.IsSdkPanelVisible SDK panel visible.
		 * @param {String} config.sdkCaption SDK panel caption.
		 * @param {String} config.sdkIcon SDK icon config.
		 * @param {Boolean} config.IsSocialAccountsPanelVisible Social accounts panel visible.
		 * @param {String} config.socialAccountsCaption Social accounts panel caption.
		 * @return {Object} Accounts links container config.
		 */
		generateTerrasoftAccountsLinks: function(config) {
			var socialAccountItems = this.generateSocialAccountButtons(config.socialAccounts);
			var gettingStartedIcon = Terrasoft.ImageUrlBuilder.getUrl(config.gettingStartedIcon);
			var communityIcon = Terrasoft.ImageUrlBuilder.getUrl(config.communityIcon);
			var marketplaceIcon = Terrasoft.ImageUrlBuilder.getUrl(config.marketplaceIcon);
			var academyIcon = Terrasoft.ImageUrlBuilder.getUrl(config.academyIcon);
			var sdkIcon = Terrasoft.ImageUrlBuilder.getUrl(config.sdkIcon);
			var salesloftIcon = Terrasoft.ImageUrlBuilder.getUrl(config.salesloftIcon);
			var wrapClassName = ["references-container", "right-item"];
			if (!Ext.isEmpty(config.wrapClassName)) {
				var customClasses = Ext.isArray(config.wrapClassName)
					? config.wrapClassName
					: [config.wrapClassName];
				wrapClassName = wrapClassName.concat(customClasses);
			}
			var gettingStartedTemplate = this.generateTerrasoftLinkLabelTemplate({
				"code": "gettingStarted",
				"icon": gettingStartedIcon,
				"label": config.gettingStartedLabel
			});
			var academyTemplate = this.generateTerrasoftLinkLabelTemplate({
				"code": "academy",
				"icon": academyIcon,
				"label": config.academyLabel
			});
			var communityTemplate = this.generateTerrasoftLinkLabelTemplate({
				"code": "community",
				"icon": communityIcon,
				"label": config.communityLabel
			});
			var marketplaceTemplate = this.generateTerrasoftLinkLabelTemplate({
				"code": "marketplace",
				"icon": marketplaceIcon,
				"label": config.marketplaceLabel
			});
			var sdkTemplate = this.generateTerrasoftLinkLabelTemplate({
				"code": "sdk",
				"icon": sdkIcon,
				"label": config.sdkCaption
			});
			var salesloftTemplate = this.generateTerrasoftLinkLabelTemplate({
				"code": "salesloft",
				"icon": salesloftIcon,
				"label": config.salesloftLabel
			});
			var gettingStartedContainer = this.generateTerrasoftLinkContainerConfig({
				"code": "gettingStarted",
				"visible": config.isGettingStartedVisible,
				"caption": config.gettingStartedCaption,
				"labelTemplate": gettingStartedTemplate,
				"markerValue": "GettingStartedContainer"
			});
			var academyContainer = this.generateTerrasoftLinkContainerConfig({
				"code": "academy",
				"visible": config.isAcademyVisible,
				"caption": config.academyCaption,
				"labelTemplate": academyTemplate,
				"markerValue": "AcademyContainer"
			});
			var communityContainer = this.generateTerrasoftLinkContainerConfig({
				"code": "community",
				"visible": config.IsComunityPanelVisible,
				"caption": config.communityCaption,
				"labelTemplate": communityTemplate,
				"markerValue": "CommunityContainer"
			});
			var marketplaceContainer = this.generateTerrasoftLinkContainerConfig({
				"code": "marketplace",
				"visible": config.isMarketplacePanelVisible,
				"caption": config.marketplaceCaption,
				"labelTemplate": marketplaceTemplate,
				"markerValue": "MarketplaceContainer"
			});
			var sdkContainer = this.generateTerrasoftLinkContainerConfig({
				"code": "sdk",
				"visible": config.IsSdkPanelVisible,
				"caption": config.sdkCaption,
				"labelTemplate": sdkTemplate,
				"markerValue": "SdkContainer"
			});
			var salesloftContainer = this.generateTerrasoftLinkContainerConfig({
				"code": "salesloft",
				"visible": true,
				"caption": config.salesloftCaption,
				"labelTemplate": salesloftTemplate,
				"markerValue": "SalesloftContainer"
			});
			var result = {
				className: "Terrasoft.Container",
				classes: {
					wrapClassName: wrapClassName
				},
				markerValue: config.markerValue || "references-container",
				items: [
					sdkContainer,
					gettingStartedContainer,
					academyContainer,
					communityContainer,
					marketplaceContainer,
					salesloftContainer,
					{
						className: "Terrasoft.Container",
						classes: {wrapClassName: ["main-social-networks-container"]},
						visible: config.IsSocialAccountsPanelVisible,
						items: [
							{
								className: "Terrasoft.Label",
								classes: {labelClass: ["social-networks-caption"]},
								caption: config.socialAccountsCaption
							},
							socialAccountItems
						]
					}
				]
			};
			return result;
		},

		/**
		 * Generate social account buttons container config.
		 * @param {Array} itemsCfg Items config.
		 * @return {Object} Social account buttons container config.
		 */
		generateSocialAccountButtons: function(itemsCfg) {
			var socialAccountContainer = {
				className: "Terrasoft.Container",
				classes: {
					wrapClassName: ["social-networks-container"]
				}
			};
			var items = [];
			Terrasoft.each(itemsCfg, function(item) {
				var itemIcon = Terrasoft.ImageUrlBuilder.getUrl(item.icon);
				var itemCfg = {
					className: "Terrasoft.Hyperlink",
					tpl: [
						"<a id=\"{id}\" name=\"{name}\" href=\"{href}\" target=\"_blank\"" +
						" class=\"{hyperlinkClass}\" style=\"{hyperlinkStyle}\" title=\"{hint}\" type=\"{type}\">",
						"<img src=\"{imageSrc}\">",
						"</a>"
					],
					tplData: {
						imageSrc: itemIcon
					},
					href: item.href,
					classes: {
						hyperlinkClass: ["social-network"]
					},
					markerValue: item.markerValue
				};
				items.push(itemCfg);
			}, this);
			socialAccountContainer.items = items;
			return socialAccountContainer;
		}
	});

	return new Terrasoft.configuration.MsTeamsMainMenuTileGenerator();
});
