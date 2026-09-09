// Global Cytoscape instance
let cy = null;
let currentLayout = 'cose';
let currentTheme = 'light';
let highlightMode = 'both'; // 'upstream', 'downstream', 'both'
let showEdgeLabels = true;
let graphStats = { nodes: 0, edges: 0, mostReferenced: null, leastReferenced: null };

// Initialize Cytoscape
function initializeCytoscape() {
    cy = cytoscape({
        container: document.getElementById('cy-container'),
        
        style: [
            {
                selector: 'node',
                style: {
                    'background-color': 'data(color)',
                    'label': 'data(label)',
                    'width': 'data(size)',
                    'height': 'data(size)',
                    'font-size': '14px',
                    'text-valign': 'center',
                    'text-halign': 'center',
                    'text-wrap': 'wrap',
                    'text-max-width': '120px',
                    'color': '#333',
                    'text-outline-color': '#fff',
                    'text-outline-width': 2,
                    'border-width': 3,
                    'border-color': 'data(borderColor)',
                    'overlay-padding': '6px',
                    'shadow-blur': 10,
                    'shadow-color': 'rgba(0, 0, 0, 0.3)',
                    'shadow-offset-x': 2,
                    'shadow-offset-y': 2
                }
            },
            {
                selector: 'node:selected',
                style: {
                    'border-width': 5,
                    'border-color': '#FF5722',
                    'shadow-blur': 20,
                    'shadow-color': 'rgba(255, 87, 34, 0.5)'
                }
            },
            {
                selector: 'node.highlighted',
                style: {
                    'background-color': '#FFD700',
                    'border-color': '#FFA500',
                    'border-width': 4,
                    'shadow-blur': 15,
                    'shadow-color': 'rgba(255, 215, 0, 0.7)'
                }
            },
            {
                selector: 'node.dimmed',
                style: {
                    'opacity': 0.3
                }
            },
            {
                selector: 'edge',
                style: {
                    'width': 3,
                    'line-color': 'data(color)',
                    'target-arrow-color': 'data(color)',
                    'target-arrow-shape': 'triangle',
                    'curve-style': 'bezier',
                    'arrow-scale': 1.5,
                    'label': 'data(label)',
                    'font-size': '10px',
                    'text-rotation': 'autorotate',
                    'text-margin-y': -10,
                    'color': '#666',
                    'text-outline-color': '#fff',
                    'text-outline-width': 1
                }
            },
            {
                selector: 'edge:selected',
                style: {
                    'width': 4,
                    'line-color': '#FF5722',
                    'target-arrow-color': '#FF5722'
                }
            },
            {
                selector: 'edge.highlighted',
                style: {
                    'width': 4,
                    'line-color': '#4CAF50',
                    'target-arrow-color': '#4CAF50'
                }
            },
            {
                selector: 'edge.dimmed',
                style: {
                    'opacity': 0.2
                }
            }
        ],

        layout: {
            name: 'cose',
            animate: true,
            animationDuration: 500
        },

        // Interaction settings
        minZoom: 0.1,
        maxZoom: 3,
        wheelSensitivity: 0.2,
        
        // Enable middle-click pan
        panningEnabled: true,
        userPanningEnabled: true,
        boxSelectionEnabled: false
    });

    // Enable middle-click pan
    cy.userPanningEnabled(true);

    // Setup event handlers
    setupEventHandlers();
    setupMiddleClickPan();
    setupDoubleClickActions();
}

// Setup event handlers for interactions
function setupEventHandlers() {
    const tooltip = document.getElementById('node-tooltip');

    // Node hover - show tooltip
    cy.on('mouseover', 'node', function(evt) {
        const node = evt.target;
        const data = node.data();
        
        tooltip.innerHTML = `
            <h4>${data.label}</h4>
            <p><span class="label">Group:</span> ${data.group || 'Unknown'}</span></p>
            <p><span class="label">Referenced By:</span> ${data.referencedByCount || 0} files</p>
            <p><span class="label">References:</span> ${data.xrefCount || 0} files</p>
            ${data.filePath ? `<p><span class="label">Path:</span> ${data.filePath}</p>` : ''}
        `;
        
        tooltip.style.display = 'block';
    });

    // Update tooltip position on mouse move
    cy.on('mousemove', function(evt) {
        if (tooltip.style.display === 'block') {
            tooltip.style.left = (evt.originalEvent.pageX + 15) + 'px';
            tooltip.style.top = (evt.originalEvent.pageY + 15) + 'px';
        }
    });

    // Hide tooltip on mouse out
    cy.on('mouseout', 'node', function(evt) {
        tooltip.style.display = 'none';
    });

    // Node click - highlight connected nodes based on mode
    cy.on('tap', 'node', function(evt) {
        const node = evt.target;
        highlightNode(node, highlightMode);
    });

    // Click on background - reset highlighting
    cy.on('tap', function(evt) {
        if (evt.target === cy) {
            cy.elements().removeClass('highlighted dimmed');
        }
    });

    // Edge click - highlight source and target
    cy.on('tap', 'edge', function(evt) {
        const edge = evt.target;
        
        cy.elements().removeClass('highlighted dimmed');
        
        edge.addClass('highlighted');
        edge.source().addClass('highlighted');
        edge.target().addClass('highlighted');
        
        cy.elements().not(edge.union(edge.source()).union(edge.target())).addClass('dimmed');
    });
}

// Setup middle-click pan
function setupMiddleClickPan() {
    let isPanning = false;
    let panStartPos = { x: 0, y: 0 };
    
    cy.on('mousedown', function(evt) {
        if (evt.originalEvent.button === 1) { // Middle mouse button
            isPanning = true;
            panStartPos = { x: evt.originalEvent.clientX, y: evt.originalEvent.clientY };
            evt.originalEvent.preventDefault();
        }
    });
    
    cy.on('mousemove', function(evt) {
        if (isPanning) {
            const dx = evt.originalEvent.clientX - panStartPos.x;
            const dy = evt.originalEvent.clientY - panStartPos.y;
            cy.panBy({ x: dx, y: dy });
            panStartPos = { x: evt.originalEvent.clientX, y: evt.originalEvent.clientY };
        }
    });
    
    document.addEventListener('mouseup', function(evt) {
        if (evt.button === 1) {
            isPanning = false;
        }
    });
}

// Setup double-click actions
function setupDoubleClickActions() {
    // Double-click on node to center and zoom
    cy.on('dbltap', 'node', function(evt) {
        const node = evt.target;
        cy.animate({
            center: { eles: node },
            zoom: 1.5,
            duration: 500
        });
        evt.stopPropagation();
    });
    
    // Double-click on background to fit all
    cy.on('dbltap', function(evt) {
        if (evt.target === cy) {
            fitToView();
        }
    });
    
    // Double-click on edge to highlight full path
    cy.on('dbltap', 'edge', function(evt) {
        const edge = evt.target;
        cy.elements().removeClass('highlighted dimmed');
        edge.addClass('highlighted');
        edge.source().addClass('highlighted');
        edge.target().addClass('highlighted');
        cy.elements().not(edge.union(edge.source()).union(edge.target())).addClass('dimmed');
        evt.stopPropagation();
    });
}

// Highlight node with different modes
function highlightNode(node, mode = 'both') {
    // Reset all
    cy.elements().removeClass('highlighted dimmed');
    
    // Highlight the selected node
    node.addClass('highlighted');
    
    let nodesToHighlight = cy.collection().union(node);
    let edgesToHighlight = cy.collection();
    
    if (mode === 'downstream' || mode === 'both') {
        // Show files that reference this node (incoming edges)
        const incomers = node.incomers('edge');
        edgesToHighlight = edgesToHighlight.union(incomers);
        nodesToHighlight = nodesToHighlight.union(incomers.sources());
    }
    
    if (mode === 'upstream' || mode === 'both') {
        // Show files this node references (outgoing edges)
        const outgoers = node.outgoers('edge');
        edgesToHighlight = edgesToHighlight.union(outgoers);
        nodesToHighlight = nodesToHighlight.union(outgoers.targets());
    }
    
    // Apply highlighting
    nodesToHighlight.addClass('highlighted');
    edgesToHighlight.addClass('highlighted');
    
    // Dim everything else
    cy.nodes().not(nodesToHighlight).addClass('dimmed');
    cy.edges().not(edgesToHighlight).addClass('dimmed');
}

// Toggle theme
function toggleTheme() {
    currentTheme = currentTheme === 'light' ? 'dark' : 'light';
    document.body.classList.toggle('dark-theme');
    
    // Update graph background
    const bgColor = currentTheme === 'dark' ? '#1a1a1a' : '#f5f7fa';
    document.getElementById('cy-container').style.background = 
        currentTheme === 'dark' ? 
        'linear-gradient(135deg, #1a1a1a 0%, #2d2d2d 100%)' : 
        'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)';
    
    return currentTheme;
}

// Toggle edge labels
function toggleEdgeLabels(show) {
    // If show parameter is provided, use it; otherwise toggle
    if (show !== undefined) {
        showEdgeLabels = show;
    } else {
        showEdgeLabels = !showEdgeLabels;
    }
    cy.edges().style('label', showEdgeLabels ? 'data(label)' : '');
    return showEdgeLabels;
}

// Filter by discipline
function filterByDiscipline(discipline) {
    if (!cy) return;
    
    cy.elements().removeClass('hidden');
    
    if (discipline && discipline !== 'all') {
        cy.nodes().forEach(node => {
            if (node.data('group') !== discipline) {
                node.addClass('hidden');
                node.connectedEdges().addClass('hidden');
            }
        });
    }
}

// Show only important nodes (referenced by X+ files)
function filterByImportance(minReferences = 3) {
    if (!cy) return;
    
    cy.elements().removeClass('hidden');
    
    cy.nodes().forEach(node => {
        if (node.data('referencedByCount') < minReferences && node.data('xrefCount') < minReferences) {
            node.addClass('hidden');
            node.connectedEdges().addClass('hidden');
        }
    });
}

// Hide isolated nodes (orphans)
function hideOrphans() {
    if (!cy) return;
    
    cy.nodes().forEach(node => {
        if (node.degree() === 0) {
            node.addClass('hidden');
        }
    });
}

// Show isolated nodes
function showOrphans() {
    if (!cy) return;
    
    cy.nodes().forEach(node => {
        if (node.degree() === 0) {
            node.addClass('orphan');
            node.style('border-color', '#FF0000');
            node.style('border-width', 4);
        }
    });
}

// Reset all filters
function resetFilters() {
    if (!cy) return;
    cy.elements().removeClass('hidden orphan');
    cy.nodes().style('border-width', 3);
}

// Set highlight mode
function setHighlightMode(mode) {
    highlightMode = mode;
    return highlightMode;
}

// Calculate and return statistics
function calculateStatistics() {
    if (!cy) return {
        visibleNodes: 0,
        visibleEdges: 0,
        mostReferenced: "",
        leastReferenced: "",
        maxReferences: 0,
        minReferences: 0,
        averageReferences: 0
    };
    
    const visibleNodes = cy.nodes(':visible');
    const visibleEdges = cy.edges(':visible');
    
    let maxRef = 0;
    let minRef = Infinity;
    let mostRef = "";
    let leastRef = "";
    let totalRefs = 0;
    
    visibleNodes.forEach(node => {
        const refCount = node.data('referencedByCount') || 0;
        totalRefs += refCount;
        
        if (refCount > maxRef) {
            maxRef = refCount;
            mostRef = node.data('label');
        }
        if (refCount < minRef) {
            minRef = refCount;
            leastRef = node.data('label');
        }
    });
    
    // Store in global stats object
    graphStats = {
        visibleNodes: visibleNodes.length,
        visibleEdges: visibleEdges.length,
        mostReferenced: mostRef,
        leastReferenced: leastRef,
        maxReferences: maxRef,
        minReferences: minRef === Infinity ? 0 : minRef,
        averageReferences: visibleNodes.length > 0 ? totalRefs / visibleNodes.length : 0
    };
    
    return graphStats;
}

// Load graph data from C# application
function loadGraphData(graphData) {
    if (!cy) {
        initializeCytoscape();
    }

    // Clear existing graph
    cy.elements().remove();

    // Process nodes
    const cyNodes = graphData.nodes.map(node => ({
        data: {
            id: node.id,
            label: node.label,
            group: node.group,
            referencedByCount: node.referencedByCount,
            xrefCount: node.xrefCount,
            filePath: node.filePath,
            color: getNodeColor(node),
            borderColor: getNodeBorderColor(node),
            size: getNodeSize(node)
        }
    }));

    // Process edges
    const cyEdges = graphData.edges.map(edge => ({
        data: {
            id: edge.id,
            source: edge.source,
            target: edge.target,
            label: edge.label,
            color: getEdgeColor(edge)
        }
    }));

    // Add to graph
    cy.add(cyNodes);
    cy.add(cyEdges);

    // Apply layout
    applyLayout(currentLayout);
    
    // Calculate statistics
    calculateStatistics();
}

// Get node color based on properties
function getNodeColor(node) {
    if (node.group === 'Architectural') return '#2196F3';
    if (node.group === 'Structural') return '#FF9800';
    if (node.group === 'MEP') return '#4CAF50';
    if (node.group === 'Reference') return '#9C27B0';
    
    // Color by referenced count
    if (node.referencedByCount > 5) return '#F44336';
    if (node.referencedByCount > 2) return '#FF9800';
    if (node.referencedByCount > 0) return '#4CAF50';
    return '#9E9E9E';
}

// Get node border color
function getNodeBorderColor(node) {
    const baseColor = getNodeColor(node);
    // Darken the base color for border
    return baseColor;
}

// Get node size based on importance
function getNodeSize(node) {
    const baseSize = 60;
    const sizeIncrement = Math.min(node.referencedByCount * 5, 40);
    return baseSize + sizeIncrement;
}

// Get edge color
function getEdgeColor(edge) {
    return '#607D8B';
}

// Apply layout
function applyLayout(layoutName) {
    let layoutOptions = {
        name: layoutName,
        animate: true,
        animationDuration: 500,
        fit: true,
        padding: 50
    };

    // Custom options for specific layouts
    if (layoutName === 'cose') {
        layoutOptions.nodeRepulsion = 8000;
        layoutOptions.idealEdgeLength = 100;
        layoutOptions.edgeElasticity = 100;
        layoutOptions.nestingFactor = 1.2;
        layoutOptions.gravity = 1;
        layoutOptions.numIter = 1000;
    } else if (layoutName === 'dagre') {
        layoutOptions.rankDir = 'TB'; // Top to bottom
        layoutOptions.nodeSep = 100;
        layoutOptions.rankSep = 150;
    } else if (layoutName === 'concentric') {
        layoutOptions.concentric = function(node) {
            return node.data('referencedByCount') || 0;
        };
        layoutOptions.levelWidth = function() {
            return 2;
        };
    } else if (layoutName === 'breadthfirst') {
        layoutOptions.directed = true;
        layoutOptions.spacingFactor = 1.5;
    }

    cy.layout(layoutOptions).run();
}

// Change layout
function changeLayout(layoutName) {
    currentLayout = layoutName;
    if (cy && cy.elements().length > 0) {
        applyLayout(layoutName);
    }
}

// Fit graph to view
function fitToView() {
    if (cy) {
        cy.fit(null, 50);
    }
}

// Reset zoom
function resetZoom() {
    if (cy) {
        cy.zoom(1);
        cy.center();
    }
}

// Export as PNG
function exportPng(options = {}) {
    if (cy) {
        const defaultOptions = {
            output: 'blob',
            bg: options.includeBackground !== false ? 'white' : 'transparent',
            full: true,
            scale: options.scale || 2
        };
        
        const png = cy.png(defaultOptions);
        
        const url = URL.createObjectURL(png);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'xlink-graph.png';
        link.click();
        URL.revokeObjectURL(url);
    }
}

// Export as SVG
function exportSvg(options = {}) {
    if (cy) {
        const svgContent = cy.svg({
            full: true,
            scale: options.scale || 1,
            bg: options.includeBackground !== false ? 'white' : 'transparent'
        });
        
        const blob = new Blob([svgContent], { type: 'image/svg+xml' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'xlink-graph.svg';
        link.click();
        URL.revokeObjectURL(url);
    }
}

// Search nodes
function searchNodes(searchTerm) {
    if (!cy || !searchTerm) {
        cy.elements().removeClass('highlighted dimmed');
        return;
    }

    const term = searchTerm.toLowerCase();
    
    // Reset
    cy.elements().removeClass('highlighted dimmed');
    
    // Find matching nodes
    const matchingNodes = cy.nodes().filter(node => {
        const label = node.data('label').toLowerCase();
        const id = node.data('id').toLowerCase();
        return label.includes(term) || id.includes(term);
    });

    if (matchingNodes.length > 0) {
        matchingNodes.addClass('highlighted');
        cy.nodes().not(matchingNodes).addClass('dimmed');
        cy.edges().addClass('dimmed');
        
        // Center on first match
        cy.animate({
            fit: {
                eles: matchingNodes,
                padding: 100
            },
            duration: 500
        });
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initializeCytoscape();
});
