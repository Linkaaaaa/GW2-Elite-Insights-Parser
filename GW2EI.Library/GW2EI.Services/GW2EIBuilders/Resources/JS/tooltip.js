/*jshint esversion: 6 */
"use strict";

const EI_TOOLTIP_SELECTOR = '[ei-tooltip]';

class EIUnifiedTooltipManager {
    constructor() {
        this.target = null;
        this.mode = null;
        this.pointer = null;
        this.raf = null;

        this.descriptionOwner = null;

        this.renderedText = null;
        this.renderedHTML = false;

        this.tooltip = document.createElement('div');
        this.tooltip.id = 'ei-tooltip';
        this.tooltip.className = 'ei-tooltip';
        this.tooltip.setAttribute('role', 'tooltip');
        this.tooltip.setAttribute('aria-hidden', 'true');

        document.body.appendChild(this.tooltip);

        this.arrow = document.createElement('div');
        this.arrow.className = 'ei-tooltip-arrow';
        this.arrow.setAttribute('aria-hidden', 'true');

        document.body.appendChild(this.arrow);

        this.onPointerOver = this.handlePointerOver.bind(this);
        this.onPointerOut = this.handlePointerOut.bind(this);
        this.onPointerMove = this.handlePointerMove.bind(this);
        this.onPointerDown = this.handlePointerDown.bind(this);
        this.onFocusIn = this.handleFocusIn.bind(this);
        this.onFocusOut = this.handleFocusOut.bind(this);
        this.onTrack = this.track.bind(this);

        document.addEventListener('pointerover', this.onPointerOver, true);
        document.addEventListener('pointerout', this.onPointerOut, true);
        document.addEventListener('pointermove', this.onPointerMove, true);
        document.addEventListener('pointerdown', this.onPointerDown, true);
        document.addEventListener('focusin', this.onFocusIn, true);
        document.addEventListener('focusout', this.onFocusOut, true);

        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                this.hide();
            }
        });

        window.addEventListener('blur', () => this.hide());
    }

    findTrigger(node) {
        if (!node) {
            return null;
        }

        if (node.nodeType !== 1) {
            node = node.parentElement;
        }

        if (!node || typeof node.closest !== 'function') {
            return null;
        }

        return node.closest(EI_TOOLTIP_SELECTOR);
    }

    findTriggerAtPointer() {
        if (!this.pointer) {
            return null;
        }

        return this.findTrigger(document.elementFromPoint(this.pointer.x, this.pointer.y));
    }

    getContent(element) {
        if (!element.hasAttribute('ei-tooltip')) {
            return null;
        }

        return {
            text: element.getAttribute('ei-tooltip'),
            useHtml: element.getAttribute('ei-tooltip-html') !== 'true'
        };
    }

    render(content) {
        if (this.renderedText === content.text && this.renderedHTML === content.useHtml) {
            return;
        }

        if (content.useHtml) {
            // Retains the existing Bootstrap HTML-tooltip
            // behavior for ei-tooltip.
            this.tooltip.innerHTML = content.text;
        } else {
            // New tooltips and native title values are text,
            // not interpreted as HTML.
            this.tooltip.textContent = content.text;
        }

        this.renderedText = content.text;
        this.renderedHTML = content.useHtml;
    }

    attachDescription(element) {
        const ids = (element.getAttribute('aria-describedby') || '').split(/\s+/).filter(Boolean);

        if (ids.indexOf(this.tooltip.id) === -1) {
            ids.push(this.tooltip.id);

            element.setAttribute('aria-describedby', ids.join(' '));

            this.descriptionOwner = element;
        }
    }

    detachDescription() {
        const element = this.descriptionOwner;

        if (!element) {
            return;
        }

        const ids = (element.getAttribute('aria-describedby') || '').split(/\s+/).filter(id => id && id !== this.tooltip.id);

        if (ids.length) {
            element.setAttribute('aria-describedby', ids.join(' '));
        } else {
            element.removeAttribute('aria-describedby');
        }

        this.descriptionOwner = null;
    }

    releaseTarget() {
        if (!this.target) {
            return;
        }

        this.target.classList.remove('ei-tooltip-current-trigger');

        this.detachDescription();
        this.target = null;
    }

    show(element, mode) {
        if (!element || !element.isConnected) {
            return;
        }

        if (element !== this.target) {
            this.releaseTarget();
            this.target = element;

            element.classList.add('ei-tooltip-current-trigger');

            this.attachDescription(element);
        }

        this.mode = mode;

        const content = this.getContent(element);

        if (!content || content.text === '') {
            this.hide();
            return;
        }

        this.render(content);

        this.tooltip.classList.add('ei-tooltip--visible');
        this.tooltip.setAttribute('aria-hidden', 'false');

        this.position();
        this.scheduleTrack();
    }

    hide() {
        this.releaseTarget();
        this.mode = null;

        this.tooltip.classList.remove('ei-tooltip--visible');
        this.tooltip.setAttribute('aria-hidden', 'true');
        this.arrow.classList.remove('ei-tooltip-arrow--visible');

        if (this.raf !== null) {
            window.cancelAnimationFrame(this.raf);
            this.raf = null;
        }
    }

    handlePointerOver(event) {
        if (event.pointerType === 'touch') {
            return;
        }

        this.pointer = {
            x: event.clientX,
            y: event.clientY
        };

        const trigger = this.findTrigger(event.target);

        if (trigger) {
            this.show(trigger, 'pointer');
        }
    }

    handlePointerOut(event) {
        if (event.pointerType === 'touch' || this.mode !== 'pointer') {
            return;
        }

        this.pointer = {
            x: event.clientX,
            y: event.clientY
        };

        if (this.findTrigger(event.target) !== this.target) {
            return;
        }

        const next = this.findTrigger(event.relatedTarget);

        if (next) {
            this.show(next, 'pointer');
        } else {
            this.hide();
        }
    }

    handlePointerMove(event) {
        if (event.pointerType === 'touch') {
            return;
        }

        this.pointer = {
            x: event.clientX,
            y: event.clientY
        };

        if (this.mode !== 'pointer' || !this.target) {
            return;
        }

        const trigger = this.findTriggerAtPointer();

        if (trigger && trigger !== this.target) {
            this.show(trigger, 'pointer');
        } else if (!trigger && !this.target.matches(':hover')) {
            this.hide();
        }
    }

    handlePointerDown(event) {
        if (event.pointerType !== 'touch') {
            return;
        }

        this.pointer = {
            x: event.clientX,
            y: event.clientY
        };

        const trigger = this.findTrigger(event.target);

        if (!trigger) {
            if (this.mode === 'touch') {
                this.hide();
            }
            return;
        }

        if (trigger === this.target && this.mode === 'touch') {
            this.hide();
        } else {
            this.show(trigger, 'touch');
        }
    }

    handleFocusIn(event) {
        if (this.mode === 'touch') {
            return;
        }

        const trigger = this.findTrigger(event.target);

        if (trigger) {
            this.show(trigger, 'focus');
        }
    }

    handleFocusOut(event) {
        if (this.mode !== 'focus') {
            return;
        }

        if (this.findTrigger(event.target) !== this.target) {
            return;
        }

        const next = this.findTrigger(event.relatedTarget);

        if (next) {
            this.show(next, 'focus');
        } else {
            this.hide();
        }
    }

    isVisibleOnScreen(element) {
        if (!element || !element.isConnected) {
            return false;
        }

        const style = window.getComputedStyle(element);

        if (style.display === 'none' ||
            style.visibility === 'hidden' ||
            style.visibility === 'collapse' ||
            style.opacity === '0'
        ) {
            return false;
        }

        const rect = element.getBoundingClientRect();

        if (rect.width <= 0 || rect.height <= 0) {
            return false;
        }

        const viewport = {
            left: 0,
            top: 0,
            right: document.documentElement.clientWidth || window.innerWidth,
            bottom: document.documentElement.clientHeight || window.innerHeight
        };

        const clip = {
            left: viewport.left,
            top: viewport.top,
            right: viewport.right,
            bottom: viewport.bottom
        };

        for (let parent = element.parentElement; parent; parent = parent.parentElement) {
            const parentStyle = window.getComputedStyle(parent);
            const parentRect = parent.getBoundingClientRect();

            if (parentStyle.overflowX !== 'visible') {
                clip.left = Math.max(clip.left, parentRect.left);
                clip.right = Math.min(clip.right, parentRect.right);
            }

            if (parentStyle.overflowY !== 'visible') {
                clip.top = Math.max(clip.top, parentRect.top);
                clip.bottom = Math.min(clip.bottom, parentRect.bottom);
            }
        }

        return (
            rect.right > clip.left &&
            rect.left < clip.right &&
            rect.bottom > clip.top &&
            rect.top < clip.bottom
        );
    }

    position() {
        if (!this.target) {
            return;
        }

        const anchor = this.target.getBoundingClientRect();
        const tip = this.tooltip.getBoundingClientRect();

        const margin = 8;
        const gap = 8;
        const arrowHeight = 8;
        const arrowHalfWidth = 7;

        const width = document.documentElement.clientWidth || window.innerWidth;

        const height = document.documentElement.clientHeight || window.innerHeight;

        // Keep the tooltip horizontally centered on the target.
        let left = anchor.left + anchor.width / 2 - tip.width / 2;

        left = Math.max(margin, Math.min(left, width - tip.width - margin));

        const spaceAbove = anchor.top - margin;
        const spaceBelow = height - anchor.bottom - margin;

        let top;
        let direction;

        // Prefer above the target. Use below if there is more room.
        if (spaceAbove >= tip.height + gap) {
            top = anchor.top - tip.height - gap;
            direction = 'down';
        } else if (spaceBelow >= tip.height + gap) {
            top = anchor.bottom + gap;
            direction = 'up';
        } else if (spaceAbove >= spaceBelow) {
            top = Math.max(margin, anchor.top - tip.height - gap);
            direction = 'down';
        } else {
            top = Math.min(height - tip.height - margin, anchor.bottom + gap);
            direction = 'up';
        }

        // Prevent the tooltip from overflowing the viewport.
        top = Math.max(margin, Math.min(top, height - tip.height - margin));

        this.tooltip.style.left = `${Math.round(left)}px`;
        this.tooltip.style.top = `${Math.round(top)}px`;

        const targetCenter = anchor.left + anchor.width / 2;

        const arrowX = Math.max(left + arrowHalfWidth, Math.min(targetCenter, left + tip.width - arrowHalfWidth));

        this.arrow.className = `ei-tooltip-arrow ei-tooltip-arrow--${direction} ` + 'ei-tooltip-arrow--visible';
        this.arrow.style.left = `${Math.round(arrowX)}px`;

        const arrowY = direction === 'down' ? top + tip.height - 1 : top - arrowHeight + 1;

        this.arrow.style.top = `${Math.round(arrowY)}px`;
    }

    scheduleTrack() {
        if (this.target && this.raf === null) {
            this.raf = window.requestAnimationFrame(this.onTrack);
        }
    }

    track() {
        this.raf = null;

        if (!this.target) {
            return;
        }

        if (!this.isVisibleOnScreen(this.target)) {
            // If Vue replaced the element underneath the
            // pointer, transfer the tooltip to its replacement.
            const replacement = this.mode === 'pointer' ? this.findTriggerAtPointer() : null;

            if (replacement && replacement !== this.target) {
                this.show(replacement, 'pointer');
            } else {
                this.hide();
            }

            return;
        }

        if (this.mode === 'pointer') {
            const underPointer = this.findTriggerAtPointer();

            if (underPointer && underPointer !== this.target) {
                this.show(underPointer, 'pointer');
                return;
            }

            if (!underPointer && !this.target.matches(':hover')) {
                this.hide();
                return;
            }
        }

        // Refresh dynamic Vue-bound tooltip content.
        const content = this.getContent(this.target);

        if (!content || content.text === '') {
            this.hide();
            return;
        }

        this.render(content);
        this.position();
        this.scheduleTrack();
    }

    showAt(text, clientX, clientY) {
        if (text === null || text === undefined || text === '') {
            if (this.mode === 'canvas') {
                this.hide();
            }
            return;
        }

        // Exit an ordinary DOM tooltip before switching to canvas mode.
        if (this.mode !== 'canvas') {
            this.hide();
        }

        this.mode = 'canvas';

        // Actor names are plain text, never HTML.
        this.render({
            text: String(text),
            useHtml: false
        });

        this.tooltip.classList.add('ei-tooltip--visible');
        this.tooltip.setAttribute('aria-hidden', 'false');

        this.positionAtPoint(clientX, clientY);
    }

    positionAtPoint(clientX, clientY) {
        const tip = this.tooltip.getBoundingClientRect();

        const margin = 8;
        const gap = 10;
        const arrowHeight = 8;
        const arrowHalfWidth = 7;

        const viewportWidth = document.documentElement.clientWidth || window.innerWidth;
        const viewportHeight = document.documentElement.clientHeight || window.innerHeight;

        // Center the tooltip horizontally over the cursor.
        let left = clientX - tip.width / 2;

        left = Math.max(margin, Math.min(left, viewportWidth - tip.width - margin));

        // Prefer placing the tooltip above the cursor.
        let top = clientY - tip.height - gap;
        let direction = 'down';

        if (top < margin) {
            top = clientY + gap;
            direction = 'up';
        }

        // Clamp to the viewport.
        if (top + tip.height > viewportHeight - margin) {
            top = clientY - tip.height - gap;
            direction = 'down';
        }

        top = Math.max(margin, Math.min(top, viewportHeight - tip.height - margin));

        this.tooltip.style.left = `${Math.round(left)}px`;
        this.tooltip.style.top = `${Math.round(top)}px`;

        const arrowX = Math.max(left + arrowHalfWidth, Math.min(clientX, left + tip.width - arrowHalfWidth));

        this.arrow.className = `ei-tooltip-arrow ei-tooltip-arrow--${direction} ` + 'ei-tooltip-arrow--visible';

        this.arrow.style.left = `${Math.round(arrowX)}px`;

        const arrowY = direction === 'down' ? top + tip.height - 1 : top - arrowHeight + 1;

        this.arrow.style.top = `${Math.round(arrowY)}px`;
    }
}

let eiUnifiedTooltipManager = null;

function initEITooltipSystem() {
    if (eiUnifiedTooltipManager) {
        return;
    }

    if (!document.body) {
        throw new Error('EI tooltip system must initialize after document.body exists.');
    }

    eiUnifiedTooltipManager = new EIUnifiedTooltipManager();
}