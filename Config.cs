class Config {

    public bool fullscreen { get; set; }

    public Config(bool fullscreen) {
        this.fullscreen = fullscreen; 
        Console.WriteLine("ClassData: " + this.fullscreen);
    }
}