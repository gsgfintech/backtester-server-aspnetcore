function formatCategoryLabel(valueText, serialDataItem, categoryAxis) {
    return (Number(valueText) / 3600).toFixed(1);
}

function createGraphs(tradeDescriptions) {
    return tradeDescriptions.map(function (tradeDescription) {
        return {
            'title': tradeDescription,
            'valueField': tradeDescription,
            'balloonText': '[[title]]: [[value]]'
        };
    });
}

function createDataProvider(pnlSeries) {

}

function renderChart(elemId, graphs, dataProvider) {
    console.log('Rendering pnls chart');

    return AmCharts.makeChart(elemId, {
        'type': 'serial',
        'theme': 'light',
        'legend': {
            'useGraphSettings': true
        },
        'dataProvider': dataProvider,
        'synchronizeGrid': true,
        'valueAxes': [{
            'title': 'Unrealized Pnl (pips)'
        }],
        'graphs': graphs,
        'chartCursor': {
            'cursorPosition': 'mouse'
        },
        'categoryField': 'time',
        'categoryAxis': {
            'labelFunction': formatCategoryLabel,
            'title': 'Time (hours)'
        },
        'export': {
            'enabled': true,
            'position': 'bottom-right'
        }
    });
}
