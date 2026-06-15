/**
 * 医院管理系统 - 公共JS
 */

// 全局 Ajax 设置
$.ajaxSetup({
    cache: false,  // 禁用 GET 缓存，避免菜单/权限数据过期
    error: function (xhr) {
        if (xhr.status === 401) {
            window.location.href = '/Account/Login';
        }
    }
});

// 通用 HTTP 工具
var http = {
    get: function (url, data) {
        return $.ajax({ url: url, type: 'GET', data: data });
    },
    post: function (url, data) {
        return $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data)
        });
    },
    postForm: function (url, data) {
        return $.ajax({
            url: url,
            type: 'POST',
            data: data
        });
    }
};

// 过滤表单空值（去掉空字符串，避免 ?chargeType= 传到后端）
function getFormWhere(formId) {
    var where = {};
    $('#' + formId).find('input,select').each(function () {
        var name = $(this).attr('name');
        var val = $(this).val();
        if (name && val !== '' && val !== null && val !== undefined) {
            where[name] = val;
        }
    });
    return where;
}

// 通用表格搜索（用于所有列表页）
function searchTable(tableId, formId) {
    layui.table.reload(tableId, { where: getFormWhere(formId), page: { curr: 1 } });
}

// Layui Table 通用渲染
function renderLayuiTable(tableId, url, cols, where, doneCallback) {
    where = where || {};
    layui.table.render({
        elem: '#' + tableId,
        url: url,
        method: 'get',
        where: where,
        page: true,
        limit: 15,
        limits: [10, 15, 20, 50, 100],
        cols: [cols],
        request: { pageName: 'pageIndex', limitName: 'pageSize' },
        response: { statusCode: 0, countName: 'total', dataName: 'data' },
        done: function (res) {
            if (doneCallback) doneCallback(res);
        }
    });
}

// 统一操作确认与提交
function handlePost(url, data, successMsg, callback) {
    http.post(url, data).then(function (res) {
        handleResponse(res, successMsg, callback);
    }).fail(function () {
        layui.layer.msg('网络异常，请稍后重试', { icon: 2 });
    });
}

// 统一响应处理
function handleResponse(res, successMsg, callback) {
    if (res.code === 0) {
        layui.layer.msg(successMsg || res.msg || '操作成功', { icon: 1 }, function () {
            if (callback) callback(res);
        });
    } else {
        layui.layer.msg(res.msg || '操作失败', { icon: 2 });
    }
}

// 表格行删除确认
function confirmDelete(url, id, tableId) {
    layui.layer.confirm('确定要删除该记录吗？此操作不可恢复。', {
        icon: 3,
        title: '删除确认',
        btn: ['确定', '取消']
    }, function (idx) {
        layui.layer.close(idx);
        handlePost(url, id, '删除成功', function () {
            layui.table.reload(tableId);
        });
    });
}

// 表单弹窗(iframe)
function openLayer(title, url, width, height) {
    width = width || '700px';
    height = height || '500px';
    var index = layui.layer.open({
        type: 2,
        title: title,
        content: url,
        area: [width, height],
        maxmin: true
    });
    return index;
}

// 关闭弹窗并刷新表格
function closeLayerAndReload(tableId) {
    var index = parent.layer.getFrameIndex(window.name);
    parent.layer.close(index);
    // 延迟刷新，等弹窗关闭动画完成
    setTimeout(function () {
        if (tableId && parent.layui && parent.layui.table) {
            parent.layui.table.reload(tableId);
        }
    }, 300);
}

// 日期格式化
function formatDate(dateStr, format) {
    if (!dateStr) return '';
    var d = new Date(dateStr);
    format = format || 'yyyy-MM-dd HH:mm:ss';
    var o = {
        'M+': d.getMonth() + 1,
        'd+': d.getDate(),
        'H+': d.getHours(),
        'm+': d.getMinutes(),
        's+': d.getSeconds()
    };
    if (/(y+)/.test(format)) {
        format = format.replace(RegExp.$1, (d.getFullYear() + '').substr(4 - RegExp.$1.length));
    }
    for (var k in o) {
        if (new RegExp('(' + k + ')').test(format)) {
            format = format.replace(RegExp.$1, RegExp.$1.length === 1 ? o[k] : ('00' + o[k]).substr(('' + o[k]).length));
        }
    }
    return format;
}

// 金额格式化
function formatMoney(amount) {
    if (amount == null) return '0.00';
    return parseFloat(amount).toFixed(2);
}

// 关闭当前弹窗（所有 iframe 弹窗的取消按钮统一使用）
function closeCurrentLayer() {
    var idx = parent.layer.getFrameIndex(window.name);
    parent.layer.close(idx);
}

// 枚举值转文本
function getEnumText(value, enumMap) {
    return enumMap[value] || value;
}

// 日期字符串转 ISO 格式（补全前导零+日期转T分隔）："2001-9-11" → "2001-09-11"
// System.Text.Json 不认缺前导零/空格分隔的日期，会导致整个 dto 反序列化为 null
function toISODate(d) {
    if (!d) return null;
    // 已经是标准 ISO 格式则直接返回
    if (d.indexOf('T') > -1 && /^\d{4}-\d{2}-\d{2}T/.test(d)) return d;
    // 分离日期和时间部分（空格或T分隔）
    var parts = d.split(/[T ]/);
    var datePart = parts[0];
    var timePart = parts.length > 1 ? parts[1] : '';
    // 补全前导零
    var segs = datePart.split(/[-/]/);
    if (segs.length === 3) {
        datePart = segs[0] + '-' + segs[1].padStart(2, '0') + '-' + segs[2].padStart(2, '0');
    }
    return timePart ? datePart + 'T' + timePart : datePart;
}
