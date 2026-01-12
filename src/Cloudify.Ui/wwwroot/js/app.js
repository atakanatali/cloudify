window.cloudify = window.cloudify || {};
window.cloudify.scrollToBottom = (element) => {
    if (!element) {
        return;
    }

    element.scrollTop = element.scrollHeight;
};
