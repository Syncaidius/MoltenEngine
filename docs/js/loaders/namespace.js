
class NamespaceLoader extends BaseLoader {
    loadContent(elPage, dataNode, docPath) {
        this.buildMemberSection(elPage, dataNode, docPath, "Namespace");
        this.buildMemberSection(elPage, dataNode, docPath, "Class");
        this.buildMemberSection(elPage, dataNode, docPath, "Struct");
        this.buildMemberSection(elPage, dataNode, docPath, "Interface");
        this.buildMemberSection(elPage, dataNode, docPath, "Enum");
    }
}