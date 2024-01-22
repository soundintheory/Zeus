Ext.util.CSS.createStyleSheet(".x-tree-node .x-tree-node-inline-icon { height: auto !important; vertical-align: middle; } .x-tree-node-anchor { vertical-align: middle; }");

var keyUp = function(el, e) {
	var tree = stpNavigation;
	var text = this.getRawValue();

	if (e.getKey() === 40) {
		tree.getRootNode().select();
	}

	if (Ext.isEmpty(text, false)) {
		clearFilter(el);
	}

	if (text.length < 3) {
		return;
	}

	if (Ext.isEmpty(text, false)) {
		return;
	}

	if (e.getKey() === Ext.EventObject.ESC) {
		clearFilter(el);
	} else {
		el.triggers[0].show();
		showSearchResults(text);
	}
};

var clearFilter = function(el, trigger, index, e) {
	var tree = stpNavigation;

	el.setValue("");
	el.triggers[0].hide();
	
	hideSearchResults();
	tree.getRootNode().collapseChildNodes(true);
};

var siteRootNode;
var siteRootIcon;
var siteRootTitle;
var searchMode = false;

function showSearchResults(query) {
	var tree = stpNavigation;
	if (!searchMode) {
		searchMode = true;
		if (!siteRootNode) {
			siteRootNode = tree.getRootNode();
			siteRootIcon = siteRootNode.attributes.icon;
			siteRootTitle = siteRootNode.attributes.text;
			tree.on('load', function(node) {
				if (searchMode && node === siteRootNode) {
					updateSearchTitle();
				}
			});
			tree.on('beforeload', function(node) {
				if (searchMode && node === siteRootNode) {
					siteRootNode.setText('Searching...');
				}
			});
		}
		siteRootNode.setIcon('/icons/folder_magnify-png/ext.axd');
		siteRootNode.setCls('disable-context');
	}
	tree.loader.abort();
	siteRootNode.loading = false;
	tree.loader.baseParams.filter = query;
	siteRootNode.reload();
}

function hideSearchResults() {
	var tree = stpNavigation;
	if (!searchMode || !siteRootNode) {
		return false;
	}
	searchMode = false;
	siteRootNode.setText(siteRootTitle);
	siteRootNode.setIcon(siteRootIcon);
	siteRootNode.setCls('');
	delete tree.loader.baseParams.filter;
	tree.loader.abort();
	siteRootNode.loading = false;
	siteRootNode.reload();
}

function updateSearchTitle() {
	var count = countAllEnabledChildren(siteRootNode);
	siteRootNode.setText("Search Results (" + count + ")");
}

function countAllEnabledChildren(node, count) {
	count = count || 0;
	if (!!node) {
		if (!!node.childNodes && node.childNodes.length > 0) {
			for (var i = 0; i < node.childNodes.length; i++) {
				if (!node.childNodes[i].disabled) {
					count++;
				}
				count = countAllEnabledChildren(node.childNodes[i], count);
			}
		}
	}
	return count;
}