var objTypes = ["Class", "Struct", "Enum", "Interface"];
var loaders = {};

function populateIndex() {
    let di = $("#doc-index");
    di.html("");

    let keys = Object.keys(docData.Members);
    keys = keys.sort(sortStrings);

    let treePath = [di];

    keys.forEach((key, index) => {
        let memList = docData.Members[key];
        if (memList.length > 0)
            buildTreeNode(di, key, "", memList[0], treePath);
    });
}

function buildEndNode(el, title, parentPath, dataNode) {
    let curPath = `${parentPath}${(parentPath.length > 0 && title ? "." : "")}${title}`;
    let idName = toIDName(curPath);
    let targetName = toIDName(title);

    let iconHtml = getIcon(dataNode);
    el.append(` <div id="t-${idName}" class="doc-target" data-target="${parentPath}" data-target-sec="${targetName}">
                    ${iconHtml}
                    <a>${title}</a>
                </div>`);

    let target = $(`#t-${idName}`);
    target.on("click", function (e) {
        let nodePath = target.data("target");
        let node = getNode(nodePath);
        let loader = loaders[node.DocType];
        if (loader == null)
            return;

        let pTitle = getPathTitle(nodePath);
        loader.load("main-page", pTitle, node, nodePath);
    });
}

function buildTreeNode(el, title, parentPath, dataNode, treePath, empty = false) {
    let curPath = `${parentPath}${(parentPath.length > 0 && title ? "." : "")}${title}`;
    if (dataNode.DocType == "Namespace")
        title = curPath;

    let idName = toIDName(curPath);
    el.append(`<div id="i-${idName}" class="sec-namespace${(treePath.length > 1 ? "-noleft" : "")}">
                    <span class="namespace-toggle\" data-target="${curPath}">${title}</span><br/>
                    <div id="in-${idName}" class="sec-namespace-inner"></div>
                </div>`);

    let elInner = $(`#in-${idName}`);

    if (!dataNode.Members || empty == true)
        return elInner;

    let keys = Object.keys(dataNode.Members);
    let nextTreePath = [...treePath, elInner];

    keys = keys.sort(sortStrings);
    keys.forEach((mName, index) => {
        let memberArray = dataNode.Members[mName];
        if (memberArray.length == 0)
            return;

        // We're building an index tree, so we only need to know about the first entry of each member, to avoid duplicate index listings.
        let memberNode = memberArray[0];
        let memType = memberNode.DocType;

        switch (memType) {
            case "Namespace":
                let resetTreePath = [nextTreePath[0]]; // Go back to root of path
                buildTreeNode(nextTreePath[0], mName, curPath, memberNode, resetTreePath);
                break;

            case "Struct":
            case "Interface":
            case "Class":
            case "Enum":
                let categoryName = toPlural(memType);
                buildCategorizedNode(elInner, categoryName, mName, curPath, memberNode, nextTreePath);
                break;

            case "Field":
            case "Property":
            case "Event":
            case "Constructor":
            case "Method":
                let endName = toPlural(memType);
                buildCategorizedNode(elInner, endName, mName, curPath, memberNode, nextTreePath, true);
                break;
        }
    });

    return elInner;
}

function buildCategorizedNode(elParent, category, title, parentPath, dataNode, treePath, isEnd = false) {
    elParent.categories = elParent.categories || {};

    if (elParent.categories[category] == null)
        elParent.categories[category] = buildTreeNode(elParent, category, parentPath, dataNode, treePath, true);

    let elInner = elParent.categories[category];
    let nextTreePath = [...treePath, elInner];
    let curPath = `${parentPath}${(parentPath.length > 0 && category ? "." : "")}${category}`;

    if (isEnd == true)
        buildEndNode(elInner, title, parentPath, dataNode, nextTreePath);
    else
        buildTreeNode(elInner, title, parentPath, dataNode, nextTreePath);
}

function toPlural(word) {
    if (word.length == 0)
        return word;

    let last = word[word.length - 1].toLowerCase();
    switch (last) {
        case 'y':
            return word.substring(0, word.length - 1) + "ies";

        case 'h':
        case 's':
        case 'x':
        case 'z':
            return word.substring(0, word.length) + "es";
    }

    return word + "s";
}

function toIDName(str) {
    return str.replace(/(\.|>|<|,| )/g, '-');
}

function toHtml(str) {
    return str.replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function getIcon(docNode) {
    return `<img class="doc-icon" src="docs/img/${docNode.DocType.toLowerCase()}.png" title="${docNode.DocType}"
/>`;
}

function sortStrings(a, b) {
    if (a > b)
        return 1;

    if (a < b)
        return -1;

    return 0;
}

function getNode(nodePath) {
    let parts = nodePath.split(".");

    let node = docData;
    parts.forEach((p, index) => {
        let next = node.Members[p];

        if (next != null && next.length > 0)
            node = next[0];
    });

    return node;
}

function getPathTitle(nodePath) {
    let parts = nodePath.split(".");
    return parts.length > 0 ? parts[parts.length - 1] : "[No Title]";
}

function loadPage(target) {
    let nodePath = target.data("target");
    let node = getNode(nodePath);
    let loader = loaders[node.DocType];
    if (loader == null) {
        console.log(`No loader for path "${nodePath}"`);
        return;
    }

    let pTitle = getPathTitle(nodePath);
    loader.load("main-page", pTitle, node, nodePath);
}

function registerDocTargets(parent) {
    parent.find(".doc-target").on("click", function (e) {
        let target = $(e.target);
        loadPage(target);
    });
}

$(document).ready(function () {
    // Set page title
    $('#doc-title').html(docData.Name);
    $('#doc-intro').html(docData.Intro);

    loaders["Class"] = new ObjectLoader();
    loaders["Struct"] = loaders["Class"];
    loaders["Interface"] = loaders["Class"];
    loaders["Enum"] = loaders["Class"];
    loaders["Namespace"] = new NamespaceLoader();

    populateIndex();

    let toggler = document.getElementsByClassName("namespace-toggle");
    let i;

    for (i = 0; i < toggler.length; i++) {
        toggler[i].addEventListener("click", function () {
            {
                this.parentElement.querySelector(".sec-namespace-inner").classList.toggle("sec-active");
                this.classList.toggle("namespace-toggle-down");

                let target = $(this);
                loadPage(target);
            }
        });
    }
})
