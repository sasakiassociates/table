function setup () {
  createCanvas(400, 400);
  background(0);
  stroke(255);
  strokeWeight(4);
  noFill();
}

function draw () {
    if (mouseIsPressed) {
        line(mouseX, mouseY, pmouseX, pmouseY);
    }
}