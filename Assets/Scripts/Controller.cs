using System.Collections.Generic;

public class Controller {
	//Player id
	public string name {get; private set;}
	//D-Pad
	public string dHorizontal {get; private set;}
	public string dVertical {get; private set;}
	//Left joystick
	public string lHorizontal {get; private set;}
	public string lVertical {get; private set;}
	public string lClick {get; private set;}
	//Right Joystick
	public string rHorizontal {get; private set;}
	public string rVertical {get; private set;}
	public string rClick {get; private set;}
	//Buttons
	public string buttonA {get; private set;}
	public string buttonB {get; private set;}
	public string buttonX {get; private set;}
	public string buttonY {get; private set;}
	//Bumpers
	public string lBumper {get; private set;}
	public string rBumper {get; private set;}
	//Others
	public string buttonStart {get; private set;}
	public string buttonBack {get; private set;}

	public Controller(string name) {
		this.name = name;
		this.dHorizontal = name + "dHorizontal";
		this.dVertical = name + "dVertical";
		this.lHorizontal = name + "lHorizontal";
		this.lVertical = name + "lVertical";
		this.lClick = name + "lClick";
		this.rHorizontal = name + "rHorizontal";
		this.rVertical = name + "rVertical";
		this.rClick = name + "rClick";
		this.buttonA = name + "buttonA";
		this.buttonB = name + "buttonB";
		this.buttonX = name + "buttonX";
		this.buttonY = name + "buttonY";
		this.lBumper = name + "lBumper";
		this.rBumper = name + "rBumper";
		this.buttonStart = name + "buttonStart";
		this.buttonBack = name + "buttonBack";
	}
}