using LayoutLibrary.Cafe;

if (File.Exists("test.bflyt"))
{
    BflytFile bflyt = new BflytFile("test.bflyt");
    bflyt.Save("new.bflyt");
}
