<?xml version="1.0" encoding="UTF-8"?> <!--
  wadl_documentation.xsl (2008-12-09)
 
  An XSLT stylesheet for generating HTML documentation from WADL,
  by Mark Nottingham <mnot@yahoo-inc.com>.
 
  Copyright (c) 2006-2008 Yahoo! Inc.
  
  This work is licensed under the Creative Commons Attribution-ShareAlike 2.5
  License. To view a copy of this license, visit
    http://creativecommons.org/licenses/by-sa/2.5/
  or send a letter to
    Creative Commons
    543 Howard Street, 5th Floor
    San Francisco, California, 94105, USA --> <!--
 * FIXME
    - Doesn't inherit query/header params from resource/@type
    - XML schema import, include, redefine don't import --> <!--
  * TODO
    - forms
    - link to or include non-schema variable type defs (as a separate list?)
    - @href error handling -->
 
<xsl:stylesheet
 xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
 xmlns:wadl="http://research.sun.com/wadl/2006/10"
 xmlns:xs="http://www.w3.org/2001/XMLSchema"
 xmlns:html="http://www.w3.org/1999/xhtml"
 xmlns:exsl="http://exslt.org/common"
 xmlns:ns="urn:namespace"
 extension-element-prefixes="exsl"
 xmlns="http://www.w3.org/1999/xhtml"
 exclude-result-prefixes="xsl wadl xs html ns"
> 
 
    <xsl:output
        method="html"
        encoding="UTF-8"
        indent="yes"
        doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
        doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"
    />
 
    <xsl:variable name="wadl-ns">http://research.sun.com/wadl/2006/10</xsl:variable>
 
    
    <!-- expand @hrefs, @types into a full tree -->
    
    <xsl:variable name="resources">
        <xsl:apply-templates select="/wadl:application/wadl:resources" mode="expand"/>
    </xsl:variable>
    
    <xsl:template match="wadl:resources" mode="expand">
        <xsl:variable name="base">
            <xsl:choose>
                <xsl:when test="substring(@base, string-length(@base), 1) = '/'">
                    <xsl:value-of select="substring(@base, 1, string-length(@base) - 1)"/>
                </xsl:when>
                <xsl:otherwise><xsl:value-of select="@base"/></xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:element name="resources" namespace="{$wadl-ns}">
            <xsl:for-each select="namespace::*">
                <xsl:variable name="prefix" select="name(.)"/>
                <xsl:if test="$prefix">
                    <xsl:attribute name="ns:{$prefix}"><xsl:value-of select="."/></xsl:attribute>
                </xsl:if>
            </xsl:for-each>
            <xsl:apply-templates select="@*|node()" mode="expand">
                <xsl:with-param name="base" select="$base"/>
            </xsl:apply-templates>
        </xsl:element>
    </xsl:template>
    
    <xsl:template match="wadl:resource[@type]" mode="expand">
        <xsl:param name="base"></xsl:param>
        <xsl:variable name="uri" select="substring-before(@type, '#')"/>
        <xsl:variable name="id" select="substring-after(@type, '#')"/>
        <xsl:element name="resource" namespace="{$wadl-ns}">
            <xsl:choose>
                <xsl:when test="$uri">
                    <xsl:variable name="included" select="document($uri, /)"/>
                    <xsl:copy-of select="$included/descendant::wadl:resource_type[@id=$id]/@*"/>
                    <xsl:attribute name="id"><xsl:value-of select="@type"/></xsl:attribute>
                    <xsl:apply-templates select="$included/descendant::wadl:resource_type[@id=$id]/*" mode="expand">
                        <xsl:with-param name="base" select="$uri"/>
                    </xsl:apply-templates>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:copy-of select="//resource_type[@id=$id]/@*"/>
                    <xsl:attribute name="id"><xsl:value-of select="$base"/>#<xsl:value-of select="@type"/></xsl:attribute>
                    <xsl:apply-templates select="//wadl:resource_type[@id=$id]/*" mode="expand">
                        <xsl:with-param name="base" select="$base"/>
                    </xsl:apply-templates>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:apply-templates select="node()" mode="expand">
                <xsl:with-param name="base" select="$base"/>
            </xsl:apply-templates>
        </xsl:element>
    </xsl:template>
    
    <xsl:template match="wadl:*[@href]" mode="expand">
        <xsl:param name="base"></xsl:param>
        <xsl:variable name="uri" select="substring-before(@href, '#')"/>
        <xsl:variable name="id" select="substring-after(@href, '#')"/>
        <xsl:element name="{local-name()}" namespace="{$wadl-ns}">
            <xsl:copy-of select="@*"/>
            <xsl:choose>
                <xsl:when test="$uri">
                    <xsl:attribute name="id"><xsl:value-of select="@href"/></xsl:attribute>
                    <xsl:variable name="included" select="document($uri, /)"/>
                    <xsl:apply-templates select="$included/descendant::wadl:*[@id=$id]/*" mode="expand">
                        <xsl:with-param name="base" select="$uri"/>
                    </xsl:apply-templates>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:attribute name="id"><xsl:value-of select="$base"/>#<xsl:value-of select="$id"/></xsl:attribute>
                    <!-- xsl:attribute name="id"><xsl:value-of select="generate-id()"/></xsl:attribute -->
                    <xsl:attribute name="element"><xsl:value-of select="//wadl:*[@id=$id]/@element"/></xsl:attribute>
                    <xsl:attribute name="mediaType"><xsl:value-of select="//wadl:*[@id=$id]/@mediaType"/></xsl:attribute>
                    <xsl:attribute name="status"><xsl:value-of select="//wadl:*[@id=$id]/@status"/></xsl:attribute>
                    <xsl:attribute name="name"><xsl:value-of select="//wadl:*[@id=$id]/@name"/></xsl:attribute>
                    <xsl:apply-templates select="//wadl:*[@id=$id]/*" mode="expand">
                        <xsl:with-param name="base" select="$base"/>
                    </xsl:apply-templates>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:element>
    </xsl:template>
    
    <xsl:template match="node()[@id]" mode="expand">
        <xsl:param name="base"></xsl:param>
        <xsl:element name="{local-name()}" namespace="{$wadl-ns}">
            <xsl:copy-of select="@*"/>
            <xsl:attribute name="id"><xsl:value-of select="$base"/>#<xsl:value-of select="@id"/></xsl:attribute>
            <!-- xsl:attribute name="id"><xsl:value-of select="generate-id()"/></xsl:attribute -->
            <xsl:apply-templates select="node()" mode="expand">
                <xsl:with-param name="base" select="$base"/>
            </xsl:apply-templates>
        </xsl:element>
    </xsl:template>
    
    <xsl:template match="@*|node()" mode="expand">
        <xsl:param name="base"></xsl:param>
        <xsl:copy>
            <xsl:apply-templates select="@*|node()" mode="expand">
                <xsl:with-param name="base" select="$base"/>
            </xsl:apply-templates>
        </xsl:copy>
    </xsl:template>
 
<!-- debug $resources
    <xsl:template match="/">
    <xsl:copy-of select="$resources"/>
    </xsl:template> -->
        
    <!-- collect grammars (TODO: walk over $resources instead) -->
    
    <xsl:variable name="grammars">
        <xsl:copy-of select="/wadl:application/wadl:grammars/*[not(namespace-uri()=$wadl-ns)]"/>
        <xsl:apply-templates select="/wadl:application/wadl:grammars/wadl:include[@href]" mode="include-grammar"/>
        <xsl:apply-templates select="/wadl:application/wadl:resources/descendant::wadl:resource[@type]" mode="include-href"/>
        <xsl:apply-templates select="exsl:node-set($resources)/descendant::wadl:*[@href]" mode="include-href"/>
    </xsl:variable>
    
    <xsl:template match="wadl:include[@href]" mode="include-grammar">
        <xsl:variable name="included" select="document(@href, /)/*"></xsl:variable>
        <xsl:element name="wadl:include">
            <xsl:attribute name="href"><xsl:value-of select="@href"/></xsl:attribute>
            <xsl:copy-of select="$included"/> <!-- FIXME: xml-schema includes, etc -->
        </xsl:element>
    </xsl:template>
    
    <xsl:template match="wadl:*[@href]" mode="include-href">
        <xsl:variable name="uri" select="substring-before(@href, '#')"/>
        <xsl:if test="$uri">
            <xsl:variable name="included" select="document($uri, /)"/>
            <xsl:copy-of select="$included/wadl:application/wadl:grammars/*[not(namespace-uri()=$wadl-ns)]"/>
            <xsl:apply-templates select="$included/descendant::wadl:include[@href]" mode="include-grammar"/>
            <xsl:apply-templates select="$included/wadl:application/wadl:resources/descendant::wadl:resource[@type]" mode="include-href"/>
            <xsl:apply-templates select="$included/wadl:application/wadl:resources/descendant::wadl:*[@href]" mode="include-href"/>
        </xsl:if>
    </xsl:template>
    
    <xsl:template match="wadl:resource[@type]" mode="include-href">
        <xsl:variable name="uri" select="substring-before(@type, '#')"/>
        <xsl:if test="$uri">
            <xsl:variable name="included" select="document($uri, /)"/>
            <xsl:copy-of select="$included/wadl:application/wadl:grammars/*[not(namespace-uri()=$wadl-ns)]"/>
            <xsl:apply-templates select="$included/descendant::wadl:include[@href]" mode="include-grammar"/>
            <xsl:apply-templates select="$included/wadl:application/wadl:resources/descendant::wadl:resource[@type]" mode="include-href"/>
            <xsl:apply-templates select="$included/wadl:application/wadl:resources/descendant::wadl:*[@href]" mode="include-href"/>
        </xsl:if>
    </xsl:template>
    
    <!-- main template -->
        
    <xsl:template match="/wadl:application">
        <html>
            <head>
                <title>
                    <xsl:choose>
                        <xsl:when test="wadl:doc[@title]">
                            <xsl:value-of select="wadl:doc[@title][1]/@title"/>
                        </xsl:when>
                        <xsl:otherwise>My Web Application</xsl:otherwise>
                    </xsl:choose>
                </title>
                <style type="text/css">
                    body {
                        font-family: sans-serif;
                        font: 0.8em/1.4 Arial, sans-serif;
                        margin: 2em 6em;
                        width: 65em;
                    }
                    pre {
                        font-family: Courier, monospace;
                        font-weight: 500;
                        font-size: 0.8em;
                        background-color: #eef;
                        padding: 1em;
                    }
                    .methods {
                        background-color: #e4e4e4;
                        margin-top: .4em;
                        padding: .6em;
                    }
                    .methods h4 {
                        border-bottom: 1px solid #fff;
                        padding: .1em 0;
                        margin-bottom: .4em;
                        color: #0b3c97;
                        font-size: 1.1em;
                    }
                    .methods h6 {
                        color: #666;
                        text-transform: lowercase;
                        margin: .6em 0 .3em;
                    }
                    .resource {
                        margin-bottom: 2em;
                        margin-top: .4em;
                    }
                    .resource h3 {
                        margin-bottom: .4em;
                        font-size: 1.4em;
                        color: #ff5700;
                    }
                    h1 {
                        font-size: 2.5em;
                    }
                    h2 {
                        border-bottom: 1px solid black;
                        margin-top: 1em;
                        color: #666;
                        margin-bottom: 0.5em;
                        font-size: 2em;
                    }
                    h3 {
                        font-size: 1.75em;
                        margin: 0.6em 0;
                    }
                    h4 {
                        color: #666;
                        margin: 0;
                        padding: 0.3em 0;
                        border-bottom: 2px solid white;
                    }
                    h6 {
                        font-size: 1.1em;
                        color: #99a;
                        margin: 0.5em 0em 0.25em 0em;
                    }
                    dd {
                        margin-left: 1em;
                    }
                    tt {
                        font-size: 1.2em;
                    }
                    table {
                        margin-bottom: 0.5em;
                        width: 100%;
                        border-collapse: collapse;
                    }
                    th {
                        text-align: left;
                        font-weight: normal;
                        color: black;
                        border-bottom: 1px solid black;
                        padding: 3px 6px;
                    }
                    td {
                        padding: 3px 6px;
                        vertical-align: top;
                        background-color: f6f6ff;
                        font-size: 0.85em;
                    }
                    td p {
                        margin: 0px;
                    }
                    ul {
                        padding-left: 1.75em;
                    }
                    p + ul, p + ol, p + dl {
                        margin-top: 0em;
                    }
                    .optional {
                        font-weight: normal;
                        opacity: 0.75;
                    }
                </style>
                <link href="prettify/prettify.css" type="text/css" rel="stylesheet" />
                <script type="text/javascript" src="prettify/prettify.js"></script>
            </head>
            <body onload="prettyPrint()">  
                <h1>
                    <xsl:choose>
                        <xsl:when test="wadl:doc[@title]">
                            <xsl:value-of select="wadl:doc[@title][1]/@title"/>
                        </xsl:when>
                        <xsl:otherwise>My Web Application</xsl:otherwise>
                    </xsl:choose>
                </h1>
                <xsl:apply-templates select="wadl:doc"/>
                <h3>Resources, Representations &amp; Errors</h3>
                <ul>
                    <li><a href="#resources">Resources</a>
                        <xsl:apply-templates select="exsl:node-set($resources)" mode="toc"/>
                    </li>
                    <li><a href="#representations">Representations</a>
                        <ul>
                            <xsl:apply-templates select="exsl:node-set($resources)/descendant::wadl:representation" mode="toc"/>
                        </ul>
                    </li>
                    <li><a href="#faults">Errors</a>
                        <ul>
                            <xsl:apply-templates select="exsl:node-set($resources)/descendant::wadl:fault" mode="toc"/>
                        </ul>
                    </li>
                </ul>
                <h2 id="resources">Resources</h2>
                <xsl:apply-templates select="exsl:node-set($resources)" mode="list"/>
                <h2 id="representations">Representations</h2>
                <xsl:apply-templates select="exsl:node-set($resources)/descendant::wadl:representation" mode="list"/>
                <xsl:if test="exsl:node-set($resources)/descendant::wadl:fault"><h2 id="faults">Errors</h2>
                    <xsl:apply-templates select="exsl:node-set($resources)/descendant::wadl:fault" mode="list"/>
                </xsl:if>
            </body>
        </html>
    </xsl:template>
 
    <!-- Table of Contents -->
 
    <xsl:template match="wadl:resources" mode="toc">
        <xsl:variable name="base">
            <xsl:choose>
                <xsl:when test="substring(@base, string-length(@base), 1) = '/'">
                    <xsl:value-of select="substring(@base, 1, string-length(@base) - 1)"/>
                </xsl:when>
                <xsl:otherwise><xsl:value-of select="@base"/></xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <ul>
            <xsl:apply-templates select="wadl:resource" mode="toc">
                <xsl:with-param name="context"><xsl:value-of select="$base"/></xsl:with-param>
            </xsl:apply-templates>
        </ul>
    </xsl:template>
 
    <xsl:template match="wadl:resource" mode="toc">
        <xsl:param name="context"/>
        <xsl:variable name="id"><xsl:call-template name="get-id"/></xsl:variable>
        <xsl:variable name="name"><xsl:value-of select="$context"/>/<xsl:value-of select="@path"/></xsl:variable>
        <li><a href="#{$id}"><xsl:value-of select="$name"/></a>
        <xsl:if test="wadl:resource">
            <ul>
                <xsl:apply-templates select="wadl:resource" mode="toc">
                    <xsl:with-param name="context" select="$name"/>
                </xsl:apply-templates>
            </ul>
        </xsl:if>
        </li>
    </xsl:template>
 
    <xsl:template match="wadl:representation|wadl:fault" mode="toc">
        <xsl:variable name="id"><xsl:call-template name="get-id"/></xsl:variable>
        <xsl:variable name="href" select="@id"/>
        <xsl:choose>
            <xsl:when test="preceding::wadl:*[@id=$href]"/>
            <xsl:otherwise>
                <li>
                    <a href="#{$id}">
                        <xsl:call-template name="representation-name"/>
                    </a>
                </li>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
 
    <!-- Listings -->
    
    <xsl:template match="wadl:resources" mode="list">
        <xsl:variable name="base">
            <xsl:choose>
                <xsl:when test="substring(@base, string-length(@base), 1) = '/'">
                    <xsl:value-of select="substring(@base, 1, string-length(@base) - 1)"/>
                </xsl:when>
                <xsl:otherwise><xsl:value-of select="@base"/></xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:apply-templates select="wadl:resource" mode="list"/>
                
    </xsl:template>
    
    <xsl:template match="wadl:resource" mode="list">
        <xsl:param name="context"/>
        <xsl:variable name="href" select="@id"/>
        <xsl:choose>
            <xsl:when test="preceding::wadl:resource[@id=$href]"/>
            <xsl:otherwise>
                <xsl:variable name="id"><xsl:call-template name="get-id"/></xsl:variable>
                <xsl:variable name="name">
                    <xsl:value-of select="$context"/>/<xsl:value-of select="@path"/>
                    <xsl:for-each select="wadl:param[@style='matrix']">
                        <span class="optional">;<xsl:value-of select="@name"/>=...</span>
                    </xsl:for-each>
                </xsl:variable>
                <div class="resource">
                    <h3 id="{$id}">
                        <xsl:choose>
                            <xsl:when test="wadl:doc[@title]"><xsl:value-of select="wadl:doc[@title][1]/@title"/></xsl:when>
                            <xsl:otherwise>
                                <xsl:copy-of select="$name"/>
                                <xsl:for-each select="wadl:method[1]/wadl:request/wadl:param[@style='query']">
                                    <xsl:choose>
                                        <xsl:when test="@required='true'">
                                            <xsl:choose>
                                                <xsl:when test="preceding-sibling::wadl:param[@style='query']">&amp;</xsl:when>
                                                <xsl:otherwise>?</xsl:otherwise>
                                            </xsl:choose>
                                            <xsl:value-of select="@name"/>
                                        </xsl:when>
                                        <xsl:otherwise>
                                            <span class="optional">
                                                <xsl:choose>
                                                    <xsl:when test="preceding-sibling::wadl:param[@style='query']">&amp;</xsl:when>
                                                    <xsl:otherwise>?</xsl:otherwise>
                                                </xsl:choose>
                                                <xsl:value-of select="@name"/>
                                            </span>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </xsl:for-each>
                            </xsl:otherwise>
                        </xsl:choose>
                        
                    </h3>
                    <xsl:apply-templates select="wadl:doc"/>
                    <xsl:apply-templates select="." mode="param-group">
                        <xsl:with-param name="prefix">resource-wide</xsl:with-param>
                        <xsl:with-param name="style">template</xsl:with-param>
                    </xsl:apply-templates>
                    <xsl:apply-templates select="." mode="param-group">
                        <xsl:with-param name="prefix">resource-wide</xsl:with-param>
                        <xsl:with-param name="style">matrix</xsl:with-param>
                    </xsl:apply-templates>
                    <h6>Methods</h6>
                    <div class="methods">
                        <xsl:apply-templates select="wadl:method"/>
                    </div>
                </div>
                <xsl:apply-templates select="wadl:resource" mode="list">
                    <xsl:with-param name="context" select="$name"/>
                </xsl:apply-templates>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
            
    <xsl:template match="wadl:method">
        <xsl:variable name="id"><xsl:call-template name="get-id"/></xsl:variable>
        <div class="method">
            <h4 id="{$id}"><xsl:value-of select="@name"/></h4>
            <xsl:apply-templates select="wadl:doc"/>
            <xsl:apply-templates select="wadl:request"/>
            <xsl:apply-templates select="wadl:response"/>
        </div>
    </xsl:template>
 
    <xsl:template match="wadl:request">
        <xsl:apply-templates select="." mode="param-group">
            <xsl:with-param name="prefix">request</xsl:with-param>
            <xsl:with-param name="style">query</xsl:with-param>
        </xsl:apply-templates>
        <xsl:apply-templates select="." mode="param-group">
            <xsl:with-param name="prefix">request</xsl:with-param>
            <xsl:with-param name="style">header</xsl:with-param>
        </xsl:apply-templates>
        <xsl:if test="wadl:representation">
            <p><em>acceptable request representations:</em></p>
            <ul>
                <xsl:apply-templates select="wadl:representation"/>
            </ul>
        </xsl:if>
    </xsl:template>
 
    <xsl:template match="wadl:response">
        <xsl:apply-templates select="." mode="param-group">
            <xsl:with-param name="prefix">response</xsl:with-param>
            <xsl:with-param name="style">header</xsl:with-param>
        </xsl:apply-templates>
        <xsl:if test="wadl:representation">
            <p><em>available response representations:</em></p>
            <ul>
                <xsl:apply-templates select="wadl:representation"/>
            </ul>
        </xsl:if>
        <xsl:if test="wadl:fault">
            <p><em>potential faults:</em></p>
            <ul>
                <xsl:apply-templates select="wadl:fault"/>
            </ul>
        </xsl:if>
    </xsl:template>
 
    <xsl:template match="wadl:representation|wadl:fault">
        <xsl:variable name="id"><xsl:call-template name="get-id"/></xsl:variable>
        <li>
            <a href="#{$id}">
                <xsl:call-template name="representation-name"/>
            </a>
        </li>
    </xsl:template>
    
    <xsl:template match="wadl:representation|wadl:fault" mode="list">
        <xsl:variable name="id"><xsl:call-template name="get-id"/></xsl:variable>
        <xsl:variable name="href" select="@id"/>
        <xsl:variable name="expanded-name">
            <xsl:call-template name="expand-qname">
                <xsl:with-param select="@element" name="qname"/>
            </xsl:call-template>
        </xsl:variable>
        <xsl:choose>
            <xsl:when test="preceding::wadl:*[@id=$href]"/>
            <xsl:otherwise>
                <h3 id="{$id}">
                    <xsl:call-template name="representation-name"/>
                </h3>
                <xsl:apply-templates select="wadl:doc"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <xsl:template match="wadl:*" mode="param-group">
        <xsl:param name="style"/>
        <xsl:param name="prefix"></xsl:param>
        <xsl:if test="ancestor-or-self::wadl:*/wadl:param[@style=$style]">
        <h6><xsl:value-of select="$prefix"/><xsl:text> </xsl:text><xsl:value-of select="$style"/> parameters</h6>
        <table>
            <tr>
                <th style="width: 25%">parameter</th>
                <th style="width: 20%">value</th>
                <th>description</th>
           </tr>
            <xsl:apply-templates select="ancestor-or-self::wadl:*/wadl:param[@style=$style]"/>
        </table>
        </xsl:if>
    </xsl:template>
    
    <xsl:template match="wadl:param">
        <tr>
            <td>
                <p><strong><xsl:value-of select="@name"/></strong></p>
            </td>
            <td>
                <p>
                <em><xsl:call-template name="link-qname"><xsl:with-param name="qname" select="@type"/></xsl:call-template></em>
                    <xsl:if test="@required='true'"> <small> (required)</small></xsl:if>
                    <xsl:if test="@repeating='true'"> <small> (repeating)</small></xsl:if>
                </p>
                <xsl:choose>
                    <xsl:when test="wadl:option">
                        <p><em>One of:</em></p>
                        <ul>
                            <xsl:apply-templates select="wadl:option"/>
                        </ul>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:if test="@default"><p>Default: <tt><xsl:value-of select="@default"/></tt></p></xsl:if>
                        <xsl:if test="@fixed"><p>Fixed: <tt><xsl:value-of select="@fixed"/></tt></p></xsl:if>
                    </xsl:otherwise>
                </xsl:choose>
            </td>
            <td>
                <xsl:apply-templates select="wadl:doc"/>
                <xsl:if test="wadl:option[wadl:doc]">
                    <dl>
                        <xsl:apply-templates select="wadl:option" mode="option-doc"/>
                    </dl>
                </xsl:if>
                <xsl:if test="@path">
                    <ul>
                        <li>XPath to value: <tt><xsl:value-of select="@path"/></tt></li>
                        <xsl:apply-templates select="wadl:link"/>
                    </ul>
                </xsl:if>
            </td>
        </tr>
    </xsl:template>
 
    <xsl:template match="wadl:link">
        <li>
            Link: <a href="#{@resource_type}"><xsl:value-of select="@rel"/></a>
        </li>
    </xsl:template>
 
    <xsl:template match="wadl:option">
        <li>
            <tt><xsl:value-of select="@value"/></tt>
            <xsl:if test="ancestor::wadl:param[1]/@default=@value"> <small> (default)</small></xsl:if>
        </li>
    </xsl:template>
 
    <xsl:template match="wadl:option" mode="option-doc">
            <dt>
                <tt><xsl:value-of select="@value"/></tt>
                <xsl:if test="ancestor::wadl:param[1]/@default=@value"> <small> (default)</small></xsl:if>
            </dt>
            <dd>
                <xsl:apply-templates select="wadl:doc"/>
            </dd>
    </xsl:template>
 
    <xsl:template match="wadl:doc">
        <xsl:param name="inline">0</xsl:param>
        <!-- skip WADL elements -->
        <xsl:choose>
            <xsl:when test="node()[1]=text() and $inline=0">
                <xsl:apply-templates select="node()" mode="copy"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:apply-templates select="node()" mode="copy"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <!-- utilities -->
 
    <xsl:template name="get-id">
        <xsl:choose>
            <xsl:when test="@id"><xsl:value-of select="@id"/></xsl:when>
            <xsl:otherwise><xsl:value-of select="generate-id()"/></xsl:otherwise>
        </xsl:choose>
    </xsl:template>
 
    <xsl:template name="get-namespace-uri">
        <xsl:param name="context" select="."/>
        <xsl:param name="qname"/>
        <xsl:variable name="prefix" select="substring-before($qname,':')"/>
        <xsl:variable name="qname-ns-uri" select="$context/namespace::*[name()=$prefix]"/>
        <!-- nasty hack to get around libxsl's refusal to copy all namespace nodes when pushing nodesets around -->
        <xsl:choose>
            <xsl:when test="$qname-ns-uri">
                <xsl:value-of select="$qname-ns-uri"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="exsl:node-set($resources)/*[1]/attribute::*[namespace-uri()='urn:namespace' and local-name()=$prefix]"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
 
    <xsl:template name="get-element">
        <xsl:param name="context" select="."/>
        <xsl:param name="qname"/>
        <xsl:variable name="ns-uri">
            <xsl:call-template name="get-namespace-uri">
                <xsl:with-param name="context" select="$context"/>
                <xsl:with-param name="qname" select="$qname"/>
            </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="localname" select="substring-after($qname, ':')"/>
        <xsl:variable name="definition" 
select="exsl:node-set($grammars)/descendant::xs:element[@name=$localname][ancestor-or-self::*[@targetNamespace=$ns-uri]]"/>
        <xsl:variable name='source' select="$definition/ancestor-or-self::wadl:include[1]/@href"/>
        <p><em>Source: <a href="{$source}"><xsl:value-of select="$source"/></a></em></p>
        <pre><xsl:apply-templates select="$definition" mode="encode"/></pre>
    </xsl:template>
 
    <xsl:template name="link-qname">
        <xsl:param name="context" select="."/>
        <xsl:param name="qname"/>
        <xsl:variable name="ns-uri">
            <xsl:call-template name="get-namespace-uri">
                <xsl:with-param name="context" select="$context"/>
                <xsl:with-param name="qname" select="$qname"/>
            </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="localname" select="substring-after($qname, ':')"/>
        <xsl:choose>
            <xsl:when test="$ns-uri='http://www.w3.org/2001/XMLSchema'">
                <a href="http://www.w3.org/TR/xmlschema-2/#{$localname}"><xsl:value-of select="$localname"/></a>
            </xsl:when>
            <xsl:otherwise>
                <xsl:variable name="definition" 
select="exsl:node-set($grammars)/descendant::xs:*[@name=$localname][ancestor-or-self::*[@targetNamespace=$ns-uri]]"/>
                <a href="{$definition/ancestor-or-self::wadl:include[1]/@href}" 
title="{$definition/descendant::xs:documentation/descendant::text()}"><xsl:value-of select="$localname"/></a>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
 
    <xsl:template name="expand-qname">
        <xsl:param name="context" select="."/>
        <xsl:param name="qname"/>
        <xsl:variable name="ns-uri">
            <xsl:call-template name="get-namespace-uri">
                <xsl:with-param name="context" select="$context"/>
                <xsl:with-param name="qname" select="$qname"/>
            </xsl:call-template>
        </xsl:variable>
        <xsl:text>{</xsl:text>
        <xsl:value-of select="$ns-uri"/>
        <xsl:text>} </xsl:text>
        <xsl:value-of select="substring-after($qname, ':')"/>
    </xsl:template>
    
    <xsl:template name="representation-name">
        <xsl:variable name="expanded-name">
            <xsl:call-template name="expand-qname">
                <xsl:with-param select="@element" name="qname"/>
            </xsl:call-template>
        </xsl:variable>
        <xsl:choose>
            <xsl:when test="wadl:doc[@title]">
                <xsl:value-of select="wadl:doc[@title][1]/@title"/>
                <xsl:if test="@status or @mediaType or @element"> (</xsl:if>
                <xsl:value-of select="@mediaType"/>
                <xsl:if test="@status or @mediaType or @element">)</xsl:if>
            </xsl:when>
            <xsl:otherwise>
                <xsl:if test="@status">Status Code </xsl:if><xsl:value-of select="@status"/>
                <xsl:if test="@status and @mediaType"> - </xsl:if>
                <xsl:value-of select="@mediaType"/>
                <xsl:if test="@element"> (</xsl:if>
                <abbr title="{$expanded-name}"><xsl:value-of select="@element"/></abbr>
                <xsl:if test="@element">)</xsl:if>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
        
    <!-- entity-encode markup for display -->
 
    <xsl:template match="*" mode="encode">
        <xsl:text>&lt;</xsl:text>
        <xsl:value-of select="name()"/><xsl:apply-templates select="attribute::*" mode="encode"/>
        <xsl:choose>
            <xsl:when test="*|text()">
                <xsl:text>&gt;</xsl:text>
                <xsl:apply-templates select="*|text()" mode="encode" xml:space="preserve"/>
                <xsl:text>&lt;/</xsl:text><xsl:value-of select="name()"/><xsl:text>&gt;</xsl:text>
            </xsl:when>
            <xsl:otherwise>
                <xsl:text>/&gt;</xsl:text>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <xsl:template match="@*" mode="encode">
        <xsl:text> </xsl:text><xsl:value-of select="name()"/><xsl:text>="</xsl:text><xsl:value-of select="."/><xsl:text>"</xsl:text>
    </xsl:template>
    
    <xsl:template match="text()" mode="encode">
        <xsl:value-of select="." xml:space="preserve"/>
    </xsl:template>
 
    <!-- copy HTML for display -->
    
    <xsl:template match="html:*" mode="copy">
        <!-- remove the prefix on HTML elements -->
        <xsl:element name="{local-name()}">
            <xsl:for-each select="@*">
                <xsl:attribute name="{local-name()}"><xsl:value-of select="."/></xsl:attribute>
            </xsl:for-each>
            <xsl:apply-templates select="node()" mode="copy"/>
        </xsl:element>
    </xsl:template>
    
    <xsl:template match="@*|node()[namespace-uri()!='http://www.w3.org/1999/xhtml']" mode="copy">
        <!-- everything else goes straight through -->
        <xsl:copy>
            <xsl:apply-templates select="@*|node()" mode="copy"/>
        </xsl:copy>
    </xsl:template>
 
</xsl:stylesheet>
