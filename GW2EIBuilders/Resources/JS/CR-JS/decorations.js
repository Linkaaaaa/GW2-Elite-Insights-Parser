/*jshint esversion: 6 */
/* jshint node: true */
/*jslint browser: true */
/*global animator, ToRadians, facingIcon, animateCanvas, noUpdateTime*/
"use strict";
//// BASE MECHANIC

function interpolatedPositionFetcher(connection, master) {
    var index = -1;
    var totalPoints = connection.positions.length / 3;
    for (var i = 0; i < totalPoints; i++) {
        var posTime = connection.positions[3 * i + 2];
        if (time < posTime) {
            break;
        }
        index = i;
    }
    if (index === -1) {
        return {
            x: connection.positions[0],
            y: connection.positions[1]
        };
    } else if (index === totalPoints - 1) {
        return {
            x: connection.positions[3 * index],
            y: connection.positions[3 * index + 1]
        };
    } else {
        var cur = {
            x: connection.positions[3 * index],
            y: connection.positions[3 * index + 1]
        };
        var curTime = connection.positions[3 * index + 2];
        var next = {
            x: connection.positions[3 * (index + 1)],
            y: connection.positions[3 * (index + 1) + 1]
        };
        var nextTime = connection.positions[3 * (index + 1) + 2];
        var pt = {
            x: 0,
            y: 0
        };
        pt.x = cur.x + (time - curTime) / (nextTime - curTime) * (next.x - cur.x);
        pt.y = cur.y + (time - curTime) / (nextTime - curTime) * (next.y - cur.y);
        return pt;
    }
}

function staticPositionFetcher(connection, master) {
    return {
        x: connection[0],
        y: connection[1]
    };
}

function masterPositionFetcher(connection, master) {
    if (!master) {
        return null;
    }
    return master.getPosition();
}

class MechanicDrawable {
    constructor(start, end, connectedTo) {
        this.start = start;
        this.end = end;
        this.positionFetcher = null;
        this.connectedTo = connectedTo;
        if (connectedTo.interpolationMethod >= 0) {
            this.positionFetcher = interpolatedPositionFetcher;
        } else if (connectedTo instanceof Array) {
            this.positionFetcher = staticPositionFetcher;
        } else {
            this.positionFetcher = masterPositionFetcher;
        }
        this.master = null;
        // Skill mode
        this.ownerID = null;
        this.owner = null;
        this.category = 0;
    }

    usingSkillMode(ownerID, category) {
        this.ownerID = ownerID;
        this.category = category;
        return this;
    }

    draw() {
        console.error("Draw should be overriden");
        // to override
    }

    getPosition() {
        var time = animator.reactiveDataStatus.time;
        if (this.start !== -1 && (this.start > time || this.end < time)) {
            return null;
        }
        return this.positionFetcher(this.connectedTo, this.master);
    }

    canDraw() {
        if (this.connectedTo === null) {
            return false;
        }
        if (this.positionFetcher === masterPositionFetcher) {
            if (this.master === null) {
                let masterId = this.connectedTo;
                this.master = animator.getActorData(masterId);
            }
            if (!this.master || !this.master.canDraw()) {
                return false;
            }
        }
        if (this.ownerID !== null) {
            if (this.owner === null) {
                this.owner = animator.getActorData(this.ownerID);
            }
            if (!this.owner) {
                return false;
            }
            let renderMask = animator.displaySettings.skillMechanicsMask;
            let drawOnSelect = (renderMask & SkillDecorationCategory["Show On Select"]) > 0;
            renderMask &= ~SkillDecorationCategory["Show On Select"];
            if ((this.category & renderMask) > 0) {
                return true;
            } else if (drawOnSelect && (this.owner.isSelected() || (this.owner.master && this.owner.master.isSelected()))) {
                return true;
            }
            return false;
        }
        return true;
    }

}
//// FACING
class FacingMechanicDrawable extends MechanicDrawable {
    constructor(start, end, connectedTo, facingData) {
        super(start, end, connectedTo);
        this.facingData = facingData;
    }

    getInterpolatedRotation(startIndex, currentIndex) {
        const offsetedIndex = currentIndex - startIndex;
        const initialAngle = this.facingData[offsetedIndex];
        const timeValue = animator.times[currentIndex];
        var angle = 0;
        var time = animator.reactiveDataStatus.time;
        if (time - timeValue > 0 && offsetedIndex < this.facingData.length - 1) {
            const nextTimeValue = animator.times[currentIndex + 1];
            let nextAngle = this.facingData[offsetedIndex + 1];
            // Make sure the interpolation is only done on the shortest path to avoid big flips around PI or -PI radians
            if (nextAngle - initialAngle < -180) {
                nextAngle += 360.0;
            } else if (nextAngle - initialAngle > 180) {
                nextAngle -= 360.0;
            }
            angle = initialAngle + (time - timeValue) / (nextTimeValue - timeValue) * (nextAngle - initialAngle);
        } else {
            angle = initialAngle;
        }
        return angle;
    }

    canDraw() {
        if (this.facingData.length === 0) {
            return false;
        }
        return super.canDraw();
    }

    getRotation() {
        var time = animator.reactiveDataStatus.time;
        if (this.start !== -1 && (this.start > time || this.end < time)) {
            return null;
        }
        if (this.facingData.length === 1) {
            return this.facingData[0];
        }
        const lastTime = animator.times[animator.times.length - 1];
        const startIndex = Math.ceil((animator.times.length - 1) * Math.max(this.start, 0) / lastTime);
        const currentIndex = Math.floor((animator.times.length - 1) * time / lastTime);
        return this.getInterpolatedRotation(startIndex, Math.max(currentIndex, startIndex));
    }

    draw() {
        if (!this.canDraw()) {
            return;
        }
        const pos = this.getPosition();
        const rot = this.getRotation();
        if (pos === null || rot === null) {
            return;
        }
        var ctx = animator.mainContext;
        const angle = ToRadians(rot);
        ctx.save();
        ctx.translate(pos.x, pos.y);
        ctx.rotate(angle);
        const facingFullSize = 5 * this.master.getSize() / 3;
        const facingHalfSize = facingFullSize / 2;
        if (this.master !== null && animator.coneControl.enabled && this.master.isSelected()) {           
            ctx.save(); 
            var coneOpening = ToRadians(animator.coneControl.openingAngle);
            ctx.rotate(0.5 * coneOpening);
            var coneRadius = animator.inchToPixel * animator.coneControl.radius;
            ctx.beginPath();
            ctx.arc(0, 0, coneRadius, -coneOpening, 0, false);
            ctx.arc(0, 0, 0, 0, coneOpening, true);
            ctx.closePath();
            ctx.fillStyle = "rgba(0, 255, 200, 0.3)";
            ctx.fill();
            ctx.restore();
        }
        ctx.drawImage(facingIcon, -facingHalfSize, -facingHalfSize, facingFullSize, facingFullSize);
        ctx.restore();
    }
}

class FacingRectangleMechanicDrawable extends FacingMechanicDrawable {
    constructor(start, end, connectedTo, facingData, width, height, translation, color) {
        super(start, end, connectedTo, facingData);
        this.width = width;
        this.height = height;
        this.translation = translation;
        this.color = color;
    }

    draw() {
        if (!this.canDraw()) {
            return;
        }
        const pos = this.getPosition();
        const rot = this.getRotation();
        if (pos === null || rot === null) {
            return;
        }
        var ctx = animator.mainContext;
        const angle = ToRadians(rot);
        ctx.save();
        ctx.translate(pos.x, pos.y);
        ctx.rotate(angle);
        ctx.beginPath();
        ctx.rect(- 0.5 * this.width + this.translation, - 0.5 * this.height, this.width, this.height);
        ctx.fillStyle = this.color;
        ctx.fill();
        ctx.restore();
    }
}

class FacingPieMechanicDrawable extends FacingMechanicDrawable {
    constructor(start, end, connectedTo, facingData, openingAngle, radius, color) {
        super(start, end, connectedTo, facingData);
        this.openingAngle = ToRadians(openingAngle);
        this.halfOpeningAngle = ToRadians(0.5 * openingAngle);
        this.radius = radius;
        this.color = color;
    }

    draw() {
        if (!this.canDraw()) {
            return;
        }
        const pos = this.getPosition();
        const rot = this.getRotation();
        if (pos === null || rot === null) {
            return;
        }
        var ctx = animator.mainContext;
        const angle = ToRadians(rot);
        ctx.save();
        ctx.translate(pos.x, pos.y);
        ctx.rotate(angle + this.halfOpeningAngle);
        ctx.beginPath();         
        ctx.arc(0, 0, this.radius, -this.openingAngle, 0, false);
        ctx.arc(0, 0, 0, 0, this.openingAngle, true);
        ctx.closePath();
        ctx.fillStyle = this.color;
        ctx.fill();
        ctx.restore();
    }
}
//// FORMS
class FormMechanicDrawable extends MechanicDrawable {
    constructor(start, end, fill, growing, color, connectedTo) {
        super(start, end, connectedTo);
        this.fill = fill;
        this.growing = growing;
        this.color = color;
    }

    getPercent() {
        if (this.growing === 0) {
            return 1.0;
        }
        var time = animator.reactiveDataStatus.time;
        var value = Math.min((time - this.start) / (Math.abs(this.growing) - this.start), 1.0);
        if (this.growing < 0) {
            value = 1 - value;
        }
        return value;
    }
}

class CircleMechanicDrawable extends FormMechanicDrawable {
    constructor(start, end, fill, growing, color, radius, connectedTo, minRadius) {
        super(start, end, fill, growing, color, connectedTo);
        this.radius = radius;
        this.minRadius = minRadius;
    }

    draw() {
        if (!this.canDraw()) {
            return;
        }
        const pos = this.getPosition();
        if (pos === null) {
            return;
        }
        var ctx = animator.mainContext;
        ctx.beginPath();
        ctx.arc(pos.x, pos.y, this.getPercent() * (this.radius - this.minRadius) + this.minRadius, 0, 2 * Math.PI);
        if (this.fill) {
            ctx.fillStyle = this.color;
            ctx.fill();
        } else {
            ctx.lineWidth = (2 / animator.scale).toString();
            ctx.strokeStyle = this.color;
            ctx.stroke();
        }
    }
}

class DoughnutMechanicDrawable extends FormMechanicDrawable {
    constructor(start, end, fill, growing, color, innerRadius, outerRadius, connectedTo) {
        super(start, end, fill, growing, color, connectedTo);
        this.outerRadius = outerRadius;
        this.innerRadius = innerRadius;
    }

    draw() {
        if (!this.canDraw()) {
            return;
        }
        const pos = this.getPosition();
        if (pos === null) {
            return;
        }
        var ctx = animator.mainContext;
        const percent = this.getPercent();
        ctx.beginPath();

        if (this.growing < 0) {    
            ctx.arc(pos.x, pos.y, this.outerRadius , 2 * Math.PI, 0, false);
            ctx.arc(pos.x, pos.y, this.innerRadius + percent * (this.outerRadius - this.innerRadius), 0, 2 * Math.PI, true);
        }  else {
            ctx.arc(pos.x, pos.y, this.innerRadius + percent * (this.outerRadius - this.innerRadius), 2 * Math.PI, 0, false);
            ctx.arc(pos.x, pos.y, this.innerRadius, 0, 2 * Math.PI, true);
        }
        ctx.closePath();
        if (this.fill) {
            ctx.fillStyle = this.color;
            ctx.fill();
        } else {
            ctx.lineWidth = (2 / animator.scale).toString();
            ctx.strokeStyle = this.color;
            ctx.stroke();
        }
    }
}

class RectangleMechanicDrawable extends FormMechanicDrawable {
    constructor(start, end, fill, growing, color, width, height, connectedTo) {
        super(start, end, fill, growing, color, connectedTo);
        this.height = height;
        this.width = width;
    }

    draw() {
        if (!this.canDraw()) {
            return;
        }
        const pos = this.getPosition();
        if (pos === null) {
            return;
        }
        var ctx = animator.mainContext;
        const percent = this.getPercent();
        ctx.beginPath();
        ctx.rect(pos.x - 0.5 * percent * this.width, pos.y - 0.5 * percent * this.height, percent * this.width, percent * this.height);
        if (this.fill) {
            ctx.fillStyle = this.color;
            ctx.fill();
        } else {
            ctx.lineWidth = (2 / animator.scale).toString();
            ctx.strokeStyle = this.color;
            ctx.stroke();
        }
    }
}

class RotatedRectangleMechanicDrawable extends RectangleMechanicDrawable {
    constructor(start, end, fill, growing, color, width, height, rotation, translation, spinangle, connectedTo) {
        super(start, end, fill, growing, color, width, height, connectedTo);
        this.rotation = ToRadians(-rotation); // positive mathematical direction, reversed since JS has downwards increasing y axis
        this.translation = translation;
        this.spinangle = ToRadians(-spinangle); // positive mathematical direction, reversed since JS has downwards increasing y axis
    }

    getSpinPercent() {
        if (this.spinangle === 0) {
            return 1.0;
        }
        var time = animator.reactiveDataStatus.time;
        return Math.min((time - this.start) / (this.end - this.start), 1.0);
    }

    draw() {
        if (!this.canDraw()) {
            return;
        }
        const pos = this.getPosition();
        if (pos === null) {
            return;
        }
        var ctx = animator.mainContext;
        const percent = this.getPercent();
        const spinPercent = this.getSpinPercent();
        const offset = {
            x: pos.x, // - 0.5 * percent * this.width,
            y: pos.y // - 0.5 * percent * this.height
        };
        const angle = this.rotation + spinPercent * this.spinangle;
        ctx.save();
        ctx.translate(offset.x, offset.y);
        ctx.rotate(angle % 360);
        ctx.beginPath();
        ctx.rect((-0.5 * this.width + this.translation) * percent, -0.5 * percent * this.height, percent * this.width, percent * this.height);
        if (this.fill) {
            ctx.fillStyle = this.color;
            ctx.fill();
        } else {
            ctx.lineWidth = (2 / animator.scale).toString();
            ctx.strokeStyle = this.color;
            ctx.stroke();
        }
        ctx.restore();
    }
}

class PieMechanicDrawable extends FormMechanicDrawable {
    constructor(start, end, fill, growing, color, direction, openingAngle, radius, connectedTo) {
        super(start, end, fill, growing, color, connectedTo);
        this.direction = ToRadians(-direction); // positive mathematical direction, reversed since JS has downwards increasing y axis
        this.halfOpeningAngle = ToRadians(0.5 * openingAngle);
        this.radius = radius;
        this.dx = Math.cos(this.direction - this.halfOpeningAngle) * this.radius;
        this.dy = Math.sin(this.direction - this.halfOpeningAngle) * this.radius;
    }

    draw() {
        if (!this.canDraw()) {
            return;
        }
        const pos = this.getPosition();
        if (pos === null) {
            return;
        }
        var ctx = animator.mainContext;
        const percent = this.getPercent();
        ctx.beginPath();
        ctx.moveTo(pos.x, pos.y);
        ctx.lineTo(pos.x + this.dx * percent, pos.y + this.dy * percent);
        ctx.arc(pos.x, pos.y, percent * this.radius, this.direction - this.halfOpeningAngle, this.direction + this.halfOpeningAngle);
        ctx.closePath();
        if (this.fill) {
            ctx.fillStyle = this.color;
            ctx.fill();
        } else {
            ctx.lineWidth = (2 / animator.scale).toString();
            ctx.strokeStyle = this.color;
            ctx.stroke();
        }
    }
}

class LineMechanicDrawable extends FormMechanicDrawable {
    constructor(start, end, fill, growing, color, connectedFrom, connectedTo) {
        super(start, end, fill, growing, color, connectedTo);
        this.connectedFrom = connectedFrom;
        this.targetPositionFetcher = null;
        if (connectedFrom.interpolationMethod >= 0) {
            this.targetPositionFetcher = interpolatedPositionFetcher;
        } else if (connectedFrom instanceof Array) {
            this.targetPositionFetcher = staticPositionFetcher;
        } else {
            this.targetPositionFetcher = masterPositionFetcher;
        }
        this.endMaster = null;
    }

    getTargetPosition() {
        var time = animator.reactiveDataStatus.time;
        if (this.start !== -1 && (this.start > time || this.end < time)) {
            return null;
        }
        return this.targetPositionFetcher(this.connectedFrom, this.endMaster);
    }
    
    canDraw() {
        if (this.connectedFrom === null) {
            return false;
        }
        if (this.targetPositionFetcher === masterPositionFetcher) {
            if (this.endMaster === null) {
                let masterId = this.connectedFrom;
                this.endMaster = animator.getActorData(masterId);
            }
            if (!this.endMaster || !this.endMaster.canDraw()) {
                return false;
            }
        }
        return super.canDraw();
    }

    draw() {
        if (!this.canDraw()) {
            return;
        }
        const pos = this.getPosition();
        const target = this.getTargetPosition();
        if (pos === null || target === null) {
            return;
        }
        var ctx = animator.mainContext;
        const percent = this.getPercent();
        ctx.beginPath();
        ctx.moveTo(pos.x, pos.y);
        ctx.lineTo(pos.x + percent * (target.x - pos.x), pos.y + percent * (target.y - pos.y));
        ctx.lineWidth = (2 / animator.scale).toString();
        ctx.strokeStyle = this.color;
        ctx.stroke();
    }
}
//// BACKGROUND
class BackgroundDrawable {
    constructor(start, end) {
        this.start = start;
        this.end = end;
    }

    draw() {
        // to override
    }

    getHeight() {
        // to override
        return 0;
    }

    getPosition() {
        // to override
        return null;
    }
}

class MovingPlatformDrawable extends BackgroundDrawable {
    constructor(start, end, image, width, height, positions) {
        super(start, end);
        this.image = new Image();
        this.image.src = image;
        this.image.onload = function () {
            animateCanvas(noUpdateTime);
        };
        this.width = width;
        this.height = height;
        this.positions = positions;
        if (this.positions.length > 1) {
            this.currentIndex = 0;
            this.currentStart = Number.NEGATIVE_INFINITY;
            this.currentEnd = positions[0][5];
        }
    }

    draw() {
        const pos = this.getInterpolatedPosition();
        if (pos === null) {
            return;
        }
        let ctx = animator.mainContext;
        const rads = pos.angle;
        ctx.save();
        ctx.translate(pos.x, pos.y);
        ctx.rotate(rads % (2 * Math.PI));
        ctx.globalAlpha = pos.opacity;
        ctx.drawImage(this.image, -0.5 * this.width, -0.5 * this.height, this.width, this.height);
        ctx.restore();
    }

    getHeight() {
        let position = this.getInterpolatedPosition();
        if (position === null) {
            return Number.NEGATIVE_INFINITY;
        }

        return position.z;
    }

    getInterpolatedPosition() {
        let time = animator.reactiveDataStatus.time;
        if (time < this.start || time > this.end) {
            return null;
        }
        if (this.positions.length === 0) {
            return null;
        }
        if (this.positions.length === 1) {
            return {
                x: this.positions[0][0],
                y: this.positions[0][1],
                z: this.positions[0][2],
                angle: this.positions[0][3],
                opacity: this.positions[0][4],
            };
        }

        let i;
        let changed = false;
        if (this.currentStart <= time && time < this.currentEnd) {
            i = this.currentIndex;
        } else {
            for (i = 0; i < this.positions.length; i++) {
                let positionTime = this.positions[i][5];
                if (positionTime > time) {
                    break;
                }
            }
            changed = true;
        }

        if (changed) {
            this.currentIndex = i;
            if (i === 0) {
                this.currentStart = Number.NEGATIVE_INFINITY;
                this.currentEnd = this.positions[0][5];
            } else {
                this.currentStart = this.positions[i - 1][5];
                if (i === this.positions.length) {
                    this.currentEnd = Number.POSITIVE_INFINITY;
                } else {
                    this.currentEnd = this.positions[i][5];
                }
            }
        }

        if (i === 0) {
            // First position is in the future
            return null;
        }

        if (i === this.positions.length) {
            // The last position is in the past, use the last position
            return {
                x: this.positions[i - 1][0],
                y: this.positions[i - 1][1],
                z: this.positions[i - 1][2],
                angle: this.positions[i - 1][3],
                opacity: this.positions[i - 1][4],
            };
        }

        let progress = (time - this.positions[i - 1][5]) / (this.positions[i][5] - this.positions[i - 1][5]);

        return {
            x: (this.positions[i - 1][0] * (1 - progress) + this.positions[i][0] * progress),
            y: (this.positions[i - 1][1] * (1 - progress) + this.positions[i][1] * progress),
            z: (this.positions[i - 1][2] * (1 - progress) + this.positions[i][2] * progress),
            angle: (this.positions[i - 1][3] * (1 - progress) + this.positions[i][3] * progress),
            opacity: (this.positions[i - 1][4] * (1 - progress) + this.positions[i][4] * progress),
        };
    }
}

class IconDecorationDrawable extends MechanicDrawable {
    constructor(start, end, connectedTo, image, pixelSize, worldSize, opacity) {
        super(start, end, connectedTo);
        this.image = new Image();
        this.image.src = image;
        this.image.onload = () => animateCanvas(noUpdateTime);
        this.pixelSize = pixelSize;
        this.worldSize = worldSize;
        this.opacity = opacity;
    }

    getSize() {
        if (animator.displaySettings.useActorHitboxWidth && this.worldSize > 0) {
            return this.worldSize;
        } else {
            return this.pixelSize / animator.scale;
        }
    }

    draw() {
        if (!this.canDraw()) {
            return;
        }
        const pos = this.getPosition();
        if (pos === null) {
            return;
        }
        
        const ctx = animator.mainContext;
        const size = this.getSize();
        ctx.save();
        ctx.globalAlpha = this.opacity;
        ctx.drawImage(this.image, pos.x - size / 2, pos.y - size / 2, size, size);
        ctx.restore();
    }
}

class IconOverheadDecorationDrawable extends IconDecorationDrawable {
    constructor(start, end, connectedTo, image, pixelSize, worldSize, opacity) {
        super(start, end, connectedTo, image, pixelSize, worldSize, opacity);
    }

    getSize() {
        if (animator.displaySettings.useActorHitboxWidth && this.worldSize > 0) {
            return this.worldSize;
        } else {
            return this.pixelSize / animator.scale;
        }
    }

    getPosition() {
        const pos = super.getPosition();
        if (!pos) {
            return null;
        }
        if (!this.master) {
            console.error('Invalid IconOverhead decoration');
            return null; 
        }
        const masterSize = this.master.getSize();
        const scale = animator.displaySettings.useActorHitboxWidth ? 1/animator.inchToPixel : animator.scale;
        pos.y -= masterSize/4 + this.getSize()/2 + 3 * overheadAnimationFrame/ maxOverheadAnimationFrame / scale;
        return pos;
    }
}
